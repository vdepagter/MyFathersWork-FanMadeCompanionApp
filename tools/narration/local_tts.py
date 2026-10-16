"""Local (open model) narration of the same scripts as elevenlabs.py, for an NVIDIA GPU.

Engines:
  qwen        Qwen3-TTS 1.7B (Apache 2.0). "Design then clone": the VoiceDesign model speaks the preview_text of
              voices.json with the voice prompt as `instruct`, that clip becomes the reference, and the Base model
              clones it for every script so the voice stays the same across passages.
  chatterbox  Chatterbox (MIT, Resemble AI). Clones a reference clip (default: the Qwen reference of the voice,
              or any MP3/WAV via --ref, e.g. an ElevenLabs preview) with an emotion "exaggeration" control.

Neither model reads ElevenLabs [audio tags]; they are stripped. Each paragraph is generated separately and
joined with a short pause.

Setup (Python 3.12, a CUDA 12.8 build of PyTorch for RTX 50xx / Blackwell):
  python -m venv .venv-tts && .venv-tts\\Scripts\\activate
  pip install torch torchaudio --index-url https://download.pytorch.org/whl/cu128
  pip install -U qwen-tts soundfile            # engine qwen
  pip install chatterbox-tts                   # engine chatterbox (may pin another torch version: use a separate venv)

  python tools/narration/local_tts.py qwen male WolvesEvil1 Accusations
  python tools/narration/local_tts.py chatterbox male WolvesEvil1 --exaggeration 0.6
Output: tools/narration/out/local-<engine>/<voice>/<script>.wav
"""
import argparse
import json
import os
import re

import numpy as np
import soundfile as sf
import torch

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "out")
TAG = re.compile(r"\[([^\]]+)\]")


def paragraphs(name):
    with open(os.path.join(HERE, "scripts", name + ".txt"), encoding="utf-8") as f:
        text = "".join(l for l in f if not l.startswith("#"))
    for p in re.split(r"\n\s*\n", text.strip()):
        tags = TAG.findall(p)
        clean = re.sub(r"\s+", " ", TAG.sub("", p)).strip()
        if clean:
            yield clean, (tags[0] if tags else None)


def join(wavs, sr, gap=0.6):
    silence = np.zeros(int(sr * gap), dtype=np.float32)
    parts = []
    for w in wavs:
        parts += [np.asarray(w, dtype=np.float32), silence]
    return np.concatenate(parts[:-1])


def load_qwen(kind, attn):
    from qwen_tts import Qwen3TTSModel
    return Qwen3TTSModel.from_pretrained(f"Qwen/Qwen3-TTS-12Hz-1.7B-{kind}", device_map="cuda:0",
                                         dtype=torch.bfloat16, attn_implementation=attn)


def qwen_reference(voice, v, attn):
    ref = os.path.join(OUT, "local-qwen", voice, "_reference.wav")
    if not os.path.exists(ref):
        os.makedirs(os.path.dirname(ref), exist_ok=True)
        model = load_qwen("VoiceDesign", attn)
        wavs, sr = model.generate_voice_design(text=v["preview_text"], language="English", instruct=v["prompt"])
        sf.write(ref, wavs[0], sr)
        del model
        torch.cuda.empty_cache()
        print(f"{ref}  (designed reference - listen to it; delete it to design again)")
    return ref


def run_qwen(voice, v, names, attn):
    ref = qwen_reference(voice, v, attn)
    model = load_qwen("Base", attn)
    ref_wav, ref_sr = sf.read(ref, dtype="float32")
    prompt = model.create_voice_clone_prompt(ref_audio=(ref_wav, ref_sr), ref_text=v["preview_text"],
                                             x_vector_only_mode=False)
    for name in names:
        wavs, sr = [], None
        for text, _tag in paragraphs(name):
            w, sr = model.generate_voice_clone(text=text, language="English", voice_clone_prompt=prompt)
            wavs.append(w[0])
        path = os.path.join(OUT, "local-qwen", voice, name + ".wav")
        sf.write(path, join(wavs, sr), sr)
        print(path)


def run_chatterbox(voice, v, names, ref, exaggeration, cfg):
    from chatterbox.tts import ChatterboxTTS
    ref = ref or os.path.join(OUT, "local-qwen", voice, "_reference.wav")
    if not os.path.exists(ref):
        raise SystemExit(f"No reference clip {ref}: run the qwen engine first or pass --ref.")
    model = ChatterboxTTS.from_pretrained(device="cuda")
    d = os.path.join(OUT, "local-chatterbox", voice)
    os.makedirs(d, exist_ok=True)
    for name in names:
        wavs = [model.generate(text, audio_prompt_path=ref, exaggeration=exaggeration, cfg_weight=cfg)
                .squeeze(0).cpu().numpy() for text, _tag in paragraphs(name)]
        path = os.path.join(d, name + ".wav")
        sf.write(path, join(wavs, model.sr), model.sr)
        print(path)


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("engine", choices=["qwen", "chatterbox"])
    ap.add_argument("voice")
    ap.add_argument("scripts", nargs="+")
    ap.add_argument("--attn", default="sdpa", help="sdpa (default) or flash_attention_2 if flash-attn is installed")
    ap.add_argument("--ref", help="chatterbox: reference clip to clone")
    ap.add_argument("--exaggeration", type=float, default=0.5)
    ap.add_argument("--cfg", type=float, default=0.4, help="chatterbox cfg_weight; lower = slower, more deliberate")
    a = ap.parse_args()
    with open(os.path.join(HERE, "voices.json"), encoding="utf-8") as f:
        v = json.load(f)[a.voice]
    if a.engine == "qwen":
        run_qwen(a.voice, v, a.scripts, a.attn)
    else:
        run_chatterbox(a.voice, v, a.scripts, a.ref, a.exaggeration, a.cfg)
