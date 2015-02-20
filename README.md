# SoundMachine
My Well Known Sound Board. Targets .NET 4.5.1+

## Getting Started
* Build (using visual studio)
* Place exe and CSCore.dll in their own folder (Program Files\SoundMachine for example).
* Create Subfolder Sounds
* Place .wav, .mp3 or other supported files in the Sounds folder (or subfolders)
* Run SoundMachine.exe
* It will hide to tray.

## Usage
* CTRL-ALT-S Shows/Hides the window, double click on tray does the same.
* Type anything to start filtering the list of sounds.
* Press enter to either play the only hit, or set focus to a random hit in the list - press enter again to play it.
* ESC will clear the search field, hitting ESC on a clear field stops all sounds.
* Numpad + and - will change the search behavior: All or Any, Full ords or Partial Words. The mode is displayed in the title bar.
* Right-clicking the tray icon will close SoundMachine.
* F5 refreshes the file list from disk (normally cached in memory)

## Details
SoundMachine prefers a sound device that has 'Speakers' in the name (and is active).
So you can have your default device set to headphones and be listening to music, yet still annoy your co-workers with sounds through your laptop speakers.

Note that you can place extra tags (filter words) in file names between square brackets. That part of the filename will be hidden in the UI, but searchable. For example 'going home [bye].mp3' will show as 'going home' but matches the 'bye' search word as well as 'going' and 'home'.
