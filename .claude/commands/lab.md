# Lab Notes Lookup

Read the relevant lab notes file based on the domain in $ARGUMENTS.

## Argument mapping

- No argument → print this reference table and stop:
  ```
  LAB_NOTES_BUILD.md       build / lint / tsconfig / test discovery errors
  LAB_NOTES_POWERSHELL.md  npm / node / gh / Windows path / multiline strings
  LAB_NOTES_GIT.md         git staging / PR creation / GitHub project board
  LAB_NOTES_I18N.md        react-i18next / locale files / translated component tests
  ```

- `build` or `b`       → read `LAB_NOTES_BUILD.md`
- `powershell` or `ps` → read `LAB_NOTES_POWERSHELL.md`
- `git` or `g`         → read `LAB_NOTES_GIT.md`
- `i18n` or `i`        → read `LAB_NOTES_I18N.md`
- `all`                → read all four files and show combined output

All lab notes files are in the project root (`d:\Documents\Proyectos\recipe-feeder\recipe-feeder-front-end\`).
