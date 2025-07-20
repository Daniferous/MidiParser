# MidiParser - Modded (Aranara-Faelei Revision)
A program to convert MIDI files into a text file to copy originally into this Scratch project: https://scratch.mit.edu/projects/406337184/

This modded variant is intended for usage for the following Scratch projects: 
1. [Pen-Based Aranara MIDI Renderer-Faelei Mod](https://daniferous.github.io/aranara-midi-player-sb3/faelei/AMRT%201.5%20Faelei%20MOD%20R3.html)

## About Mod
Initially, this program intended to add support for tempo events as the original code, made by [K9ShyGuy](https://scratch.mit.edu/users/K9shyguy/), did not include tempo changes. 

After multiple iterations in the past few years, the AraMIDI format, [which you can learn more about by clicking here](https://daniferous.github.io/aranara-midi-player-sb3/versions), now includes limited support for Control Change Events (such as Volume and Expression events), and full support for Program Change Events (or Instruments). However, do know that the Instruments only involve the standard Scratch library.

This is the "upgraded" version of this format, [known as the Faelei MIDI Format](https://github.com/Daniferous/MidiParser/tree/Faelei), which adds full Control Change and Pitch Bend Support. This is only made possible using a plug-in made by [hevean](https://scratch.mit.edu/users/hevean_3/) which allows Scratch Projects to have MIDI Synth Support.

## How to use
The compiled executable can be found [here](../MidiParser/bin/Release/netcoreapp3.1/publish):

There's a green button in the upper left area of the page. Click on that, then click "Download ZIP" Extract the zip file, then go to MidiParser >> bin >> Debug >> netcoreapp3.1 You can either run the exe file and paste in the MIDI file path, or you can drag MIDI files onto the exe, and convert them that way. I find the 2nd method easier.

For this modification, Faelei MIDI Renderers will only accept file drag-and-drop methods to import a MIDI. This is far simpler and erases the hassle of having to copy contents from within the converted MIDI.

This modded version supports Batch Modification. Dropping multiple files will output multiple converted files.

You may need to install DotNet Core Runtime: https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.10-windows-x64-installer

## Disclaimer
AraMIDI Format will continue to be supported. This is because the Faelei MIDI Format is simply an extension to the existing AraMIDI format. In theory, AraMIDIs can be run in Faelei MIDI Programs. However, it is with best of interests to have Faelei MIDI Progams only accept Faelei MIDI Formats.

Also, if you have any inquiries regarding this modification, feel free to contact me through my email, my [Scratch Profile](https://scratch.mit.edu/users/OjasnGamer101/), and the [Daniferousity Discord Server](https://discord.gg/kTD8y6YDjJ). 

## Credits
[FlynnD273](https://github.com/FlynnD273/MidiParser) for the original code.

Daniferous for the tempo events, MIDI channel,  Volume and Expresion Control Change implementation, and Instrument Integration.

Also Daniferous for the addition of full Control Change and Pitch Bend Support.

I'd like to include [hevean](https://scratch.mit.edu/users/hevean_3/) for the development of the MIDI plugin which is how this project became possible.