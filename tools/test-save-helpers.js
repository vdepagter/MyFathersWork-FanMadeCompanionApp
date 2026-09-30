// Browser-console helpers for play-testing the web app without playing from the start.
// Paste into the console on http://localhost:<port>/ (or store in localStorage.helpers and eval after reloads).
//
//   mkSave(hubId, { n: 3, years: 2, gen: 1, vars: { ... } });  location.href = "/Gameplay";
//
// mkSave writes a crafted save into localStorage "continue"; /Gameplay loads it when no game is active.
// hubId = CostOfDiseaseHubId (1 Fever, 2 Devastation, 3 Hospital, 4 GloomyGothic, 5 Prosperity, 6 NoUniversity, 7 University).
// Years: 0 Early, 1 Middle, 2 Late. Generation: 0/1/2. Enum values in vars are numeric (see Shared/Enums).
// Every per-player dictionary in TheCostOfDiseaseVars must be present, otherwise lookups throw (Reset runs before
// the save is applied, with the *old* player names). RandomArray may be omitted - Reset rolls a fresh one.
// opts.mirror = true starts in mirror mode with every random draw unanswered.
//
//   await run(["end of the Generation", "Confirm", "Click here to continue", "type:42", "Confirm"])
//
// run() clicks the last link/button whose text contains each string ("type:X" fills the input popup) and
// returns the text shown after every step.

window.mkSave = (hub, opts = {}) => {
  const n = opts.n || 3; const ps = ["Anna", "Bob", "Cleo", "Dan"].slice(0, n);
  const d = v => Object.fromEntries(ps.map(p => [p, JSON.parse(JSON.stringify(v))]));
  const v = Object.assign({
    Wolves: 0, Hunters: 1, Tracker: 7, Mayor: "Bob", Building: 1, Charity: "Cleo", FeverCure: "Anna",
    Hosp: d(true), Life: d(false), LifeCount: 0, Ally: d(0), BuildingPlay: d(0), HelpedExposeBuilding: d([false, false, false]),
    Gen2Buildings: [1, 3, 2], BuildingsExposeValue: [1, 0, 2], Letter: ["", "", "", "", "", ""],
    MwDiscipline: d(0), MwType: d(0), MwCost: d(0), MwName: d(""), MwAdjective: d(0), Scores: d(0), HubId: hub,
    ...(opts.mirror ? { RandomArray: Array(301).fill(-1) } : {})
  }, opts.vars || {});
  const g = {
    Language: "English", ScenarioLanguage: "English_Source", ScenarioId: 1, PlayersNum: n, MirrorMode: !!opts.mirror,
    PlayerAName: "Anna", PlayerBName: "Bob", PlayerCName: n > 2 ? "Cleo" : "", PlayerDName: n > 3 ? "Dan" : "",
    TownName: "Testville", Years: opts.years ?? 2, Generation: opts.gen ?? 1, TmpValues: {}, TheCostOfDiseaseVars: v
  };
  localStorage.setItem("continue", btoa(JSON.stringify(g)));
  return "ok";
};

window.clickText = t => {
  const els = [...document.querySelectorAll("a,button")].filter(e => e.innerText.includes(t));
  if (!els.length) return "NOTFOUND: " + t;
  els[els.length - 1].click();
  return "clicked";
};

window.view = () => {
  const c = document.querySelector(".popup-box") || document.querySelector(".gameplay-container");
  return c ? c.innerText : document.body.innerText;
};

window.step = async t => {
  const r = clickText(t);
  await new Promise(res => setTimeout(res, 300));
  return (r === "clicked" ? "" : r + "\n") + view();
};

window.run = async (steps, len = 250) => {
  const out = [];
  for (const t of steps) {
    if (t.startsWith("type:")) {
      const i = document.querySelector(".input-field");
      i.value = t.slice(5);
      i.dispatchEvent(new Event("input", { bubbles: true }));
      await new Promise(res => setTimeout(res, 150));
      continue;
    }
    out.push("> " + t + "\n" + (await step(t)).slice(0, len));
  }
  return out.join("\n=====\n");
};
