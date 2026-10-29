// Screen transitions (docs/AUDIO_TRANSITIONS_MAP.md section 4), driven from Pages/Gameplay.razor (OnAfterRender, the same
// screen change detection that plays the transition sounds). CSS: "Screen transitions" in css/styles.css.
//   run(kind)    - the black ink overlay #screen-transition (index.html):
//                  "base"  - story window <-> hub: covers, holds 200 ms, then recedes to the top-right corner (400 ms).
//                            The hold must match _REVEAL_DELAY_MS in Pages/Gameplay.razor (the reveal sound starts with the recede).
//                  "popup" - a new GameplayPopup: recede only (400 ms).
//   enter(kind)  - entrance animations of the elements on the new screen:
//                  "popup" (GameplayPopup), "input" (GameplayInputPopup), "same" (window -> window, hub -> hub),
//                  "page" (setup / menu page after a navigation, from Layout/MainLayout.razor).
//   scrollTop()  - back to the top of the page when a new story window or a new round's hub is shown.
// Both restart their animation (remove class, force reflow, add class) and clear it on animationend (or a timer). With
// prefers-reduced-motion the overlay is skipped; the entrances become a short opacity fade (CSS).
(function () {
    "use strict";

    // class and duration (ms, as in css/styles.css) per overlay kind
    const INK = { base: ["mfw-ink-base", 600], popup: ["mfw-ink-popup", 400] };

    // [selector, class, duration] per entrance kind
    const ENTRANCES = {
        popup: [[".popup-box:not(.input-popup):not(.mirror-popup) > .popup-content", "mfw-enter-popup-panel", 450],
                [".popup-box:not(.input-popup):not(.mirror-popup) > .popup-button", "mfw-enter-popup-button", 450]],
        input: [[".popup-box.input-popup > .popup-content", "mfw-enter-input-panel", 450],
                [".popup-box.input-popup > .popup-button", "mfw-enter-input-button", 450]],
        same:  [[".gameplay-container", "mfw-enter-fade", 200]],
        page:  [[".page-root > *", "mfw-enter-page", 350]]
    };

    const running = new WeakMap(); // element -> detach() of the animation that runs on it

    function reducedMotion() {
        return !!(window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches);
    }

    // Restarts the animation class on the element and removes it again on animationend. A timer backs that up: a hidden
    // tab does not fire animationend, and a class left behind would keep the overlay blocking input (or keep the
    // forwards fill overriding hover transforms). animationcancel is not used: restarting cancels the previous run, and
    // that event arrives after the new listener is attached.
    function play(el, cls, ms, allClasses) {
        const previous = running.get(el);
        if (previous) previous();

        allClasses.forEach(c => el.classList.remove(c));
        void el.offsetWidth; // force reflow so the animation starts again
        el.classList.add(cls);

        let timer = 0;
        const detach = () => {
            clearTimeout(timer);
            el.removeEventListener("animationend", onEnd);
            running.delete(el);
        };
        const clear = () => {
            detach();
            el.classList.remove(cls);
        };
        const onEnd = e => {
            if (e.target === el) clear(); // ignore animations of children
        };

        timer = setTimeout(clear, ms + 100);
        el.addEventListener("animationend", onEnd);
        running.set(el, detach);
    }

    function run(kind) {
        const ink = INK[kind];
        const el = document.getElementById("screen-transition");
        if (!ink || !el || reducedMotion()) return;

        play(el, ink[0], ink[1], Object.values(INK).map(i => i[0]));
    }

    function enter(kind) {
        const list = ENTRANCES[kind];
        if (!list) return;

        list.forEach(([selector, cls, ms]) => {
            document.querySelectorAll(selector).forEach(el => play(el, cls, ms, [cls]));
        });
    }

    // A new story page or round starts at the top (a long page left the next one scrolled down)
    function scrollTop() {
        window.scrollTo({ top: 0, left: 0, behavior: "instant" });
    }

    window.mfwTransitions = { run: run, enter: enter, scrollTop: scrollTop };
})();
