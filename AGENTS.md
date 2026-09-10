# Project workflow

- Treat `C:\Users\crues\source\repos\IdeologyReformation` as the authoritative repository.
- After every completed change, successfully build the mod and mirror the repository working tree to `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\IdeologyReformation`.
- Exclude only the independent `.git` directories when mirroring. All other files in the repository and local mod folder must match.
- Verify parity after deployment by comparing relative file paths and file hashes; completion requires zero missing, extra, or mismatched files.
