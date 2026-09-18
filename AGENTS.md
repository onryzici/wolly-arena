# User workflow constraint

Do not launch Unity Editor unless the user explicitly asks to open Unity. This includes graphical, batch/headless, cloned-project and test/build Editor sessions. General requests to continue development or test changes do not authorize opening Unity. Do not trigger Play/Stop in an existing session without an explicit request.

Continue with source-file work and offline compilation/tests that do not launch an Editor. Clearly distinguish offline validation from unperformed in-Editor visual, runtime and device checks.

Latest user instruction: the phone is disconnected and the user has left the computer. Do not open Unity UI or launch an Editor for the current dash and menu-color revisions; work offline only. The earlier phone build authorization does not authorize another launch for these revisions.
