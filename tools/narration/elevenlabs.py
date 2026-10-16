"""ElevenLabs narration pilot: design voices from a prompt and render tagged narration scripts.

Needs only the Python standard library. The API key is read from the environment variable
ELEVENLABS_API_KEY (never put it in a file in the repository).

  python tools/narration/elevenlabs.py design male            # 3 preview MP3s for the "male" prompt in voices.json
  python tools/narration/elevenlabs.py save male <generated_voice_id>   # keep a preview as a voice, id -> voices.local.json
  python tools/narration/elevenlabs.py render male TheCostOfDisease/WolvesEvil1         # scripts/<name>.txt -> out/male/<name>.mp3
  python tools/narration/elevenlabs.py render male TheCostOfDisease/WolvesEvil1 --plain # without audio tags -> out/male-plain/
  python tools/narration/elevenlabs.py render oliver TheCostOfDisease/Dubious --web     # 64 kbps into the web app:
                                                                    # wwwroot/audio/vo/ai/<name>.mp3 (GameplayWindow.SetAiVoiceOver)
  python tools/narration/elevenlabs.py credits                          # remaining character credits

Output goes to tools/narration/out/ (git-ignored). Scripts are plain text with ElevenLabs v3 audio tags
([whispers], [sighs], ...); lines starting with # are comments. They are kept apart from the localization CSVs
so tags never appear on screen.
"""
import base64
import json
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
API = "https://api.elevenlabs.io"
VOICES = os.path.join(HERE, "voices.json")
LOCAL = os.path.join(HERE, "voices.local.json")  # saved voice ids, git-ignored
SCRIPTS = os.path.join(HERE, "scripts")
OUT = os.path.join(HERE, "out")
WEB = os.path.join(HERE, "..", "..", "MyFathersWorkWebApp", "MyFathersWorkWebApp", "wwwroot", "audio", "vo", "ai")


def key():
    k = os.environ.get("ELEVENLABS_API_KEY")
    if not k and sys.platform == "win32":  # set with setx but this terminal started before it
        import winreg
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment") as reg:
                k = winreg.QueryValueEx(reg, "ELEVENLABS_API_KEY")[0]
        except OSError:
            pass
    if not k:
        sys.exit("Set ELEVENLABS_API_KEY first (setx ELEVENLABS_API_KEY <key>, then open a new terminal).")
    return k


def call(method, path, body=None, raw=False):
    req = urllib.request.Request(API + path, method=method,
                                 data=json.dumps(body).encode() if body is not None else None,
                                 headers={"xi-api-key": key(), "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=300) as r:
            data = r.read()
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code} on {path}: {e.read().decode(errors='replace')}")
    return data if raw else json.loads(data)


def load(path):
    if not os.path.exists(path):
        return {}
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def script_text(name):
    with open(os.path.join(SCRIPTS, name + ".txt"), encoding="utf-8") as f:
        lines = [l.rstrip() for l in f if not l.startswith("#")]
    return "\n".join(lines).strip()


def design(voice):
    v = load(VOICES)[voice]
    res = call("POST", "/v1/text-to-voice/design?output_format=mp3_44100_128", {
        "voice_description": v["prompt"],
        "model_id": "eleven_ttv_v3",
        "text": v["preview_text"],
        "guidance_scale": v.get("guidance_scale", 5),
    })
    d = os.path.join(OUT, "design", voice)
    os.makedirs(d, exist_ok=True)
    for i, p in enumerate(res["previews"], 1):
        path = os.path.join(d, f"preview{i}_{p['generated_voice_id']}.mp3")
        with open(path, "wb") as f:
            f.write(base64.b64decode(p["audio_base_64"]))
        print(f"{path}  ({p['duration_secs']:.0f} s)")
    print(f"Listen, then: python tools/narration/elevenlabs.py save {voice} <generated_voice_id>")


def save(voice, generated_id):
    v = load(VOICES)[voice]
    res = call("POST", "/v1/text-to-voice", {
        "voice_name": v["name"],
        "voice_description": v["prompt"][:1000],
        "generated_voice_id": generated_id,
    })
    local = load(LOCAL)
    local[voice] = res["voice_id"]
    with open(LOCAL, "w", encoding="utf-8") as f:
        json.dump(local, f, indent=2)
    print(f"Saved {voice} as voice {res['voice_id']}")


def render(voice, names, plain=False, web=False):
    v = load(VOICES)[voice]
    voice_id = load(LOCAL).get(voice) or v.get("voice_id")
    if not voice_id:
        sys.exit(f"No voice id for {voice}: run design + save first.")
    d = WEB if web else os.path.join(OUT, voice + ("-plain" if plain else ""))
    fmt = "mp3_44100_64" if web else "mp3_44100_128"  # web: same bitrate as the original clips
    for name in names:
        text = script_text(name)
        if plain:  # comparison render without audio tags
            text = re.sub(r"[ ]*\[[^\]]+\][ ]*", " ", text).replace("\n ", "\n").strip()
        audio = call("POST", f"/v1/text-to-speech/{voice_id}?output_format={fmt}", {
            "text": text,
            "model_id": "eleven_v3",
            "voice_settings": v.get("settings", {}),
        }, raw=True)
        path = os.path.normpath(os.path.join(d, name + ".mp3"))
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "wb") as f:
            f.write(audio)
        print(f"{path}  ({len(text)} characters)")


def library(search):
    res = call("GET", "/v1/shared-voices?page_size=20&search=" + urllib.parse.quote(search))
    for v in res["voices"]:
        print(f"{v['public_owner_id']} {v['voice_id']}  {v['name']}  ({v.get('accent')}, {v.get('age')}, {v.get('category')})")


def add(voice, owner_id, voice_id, name):
    res = call("POST", f"/v1/voices/add/{owner_id}/{voice_id}", {"new_name": name})
    local = load(LOCAL)
    local[voice] = res["voice_id"]
    with open(LOCAL, "w", encoding="utf-8") as f:
        json.dump(local, f, indent=2)
    print(f"Added {name} as {voice} ({res['voice_id']})")


def credits():
    s = call("GET", "/v1/user/subscription")
    print(f"{s['tier']}: {s['character_count']} / {s['character_limit']} characters used")


if __name__ == "__main__":
    a = sys.argv[1:]
    if not a:
        sys.exit(__doc__)
    if a[0] == "design":
        design(a[1])
    elif a[0] == "save":
        save(a[1], a[2])
    elif a[0] == "render":
        flags = {x for x in a if x.startswith("--")}
        a = [x for x in a if not x.startswith("--")]
        render(a[1], a[2:], "--plain" in flags, "--web" in flags)
    elif a[0] == "library":
        library(" ".join(a[1:]))
    elif a[0] == "add":
        add(a[1], a[2], a[3], " ".join(a[4:]) or a[1])
    elif a[0] == "credits":
        credits()
    else:
        sys.exit(__doc__)
