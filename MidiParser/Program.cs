using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows.Forms;
using System.Threading;

namespace MidiParser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Iterate through all files dropped onto the program
            Console.WriteLine("[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]\n\nMIDIParser Faelei Mod\n");
            try
            {
                switch (args.Length)
                {
                    case 1:
                        ConvertFile(args[0], 1);
                        break;
                    case 0:
                        while (true)
                        {
                            Console.WriteLine("Paste file path to convert here (Enter a blank line to quit):");
                            string s = Console.ReadLine();
                            if (s != "")
                                ConvertFile(s, 1);
                            else
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]\n");
                        Console.WriteLine("Batch Convert function is partially supported.\n");
                        //Console.ReadLine();
                        ConvertAllFiles(args);
                        break;
                }
                //Post Operation.
                Console.WriteLine("[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]\n\nFinished process.\nIf this window does not close automatically, press enter to end.\n\n[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]");
                Console.ReadLine(); //Leaving this here for the option to have the window not close automatically.
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception was thrown:\n{e.Message}\n\nPress enter to quit.");
                Console.ReadLine();
            }
        }

        //An Attempt to make Batch Conversion Possible
        private static void ConvertAllFiles (string[] files)
        {
            int count = 1;
            foreach (string file in files)
            {
                ConvertFile(file, count);
                count++;
            }
        }

        private static string ConvertFile (string file, int fileCount)
        {
            //Make sure the file exists
            if (File.Exists(file))
            {
                string path = file;
                string output = Path.Combine(Path.GetDirectoryName(path), "!" + Path.GetFileNameWithoutExtension(path) + ".faelei");
                MidiFile mid;

                //If an error is thrown, notify user and continue looping
                try
                {
                    mid = new MidiFile(path);
                    int ticksPerQuarterNote = mid.DeltaTicksPerQuarterNote;
                    //Mod Alters Resolution to better increase compatibility.
                    //MIDI TPQ is 384, though is processed as TPH (Ticks per half-note)
                    int prctmp = 60; //Formula using 120 BPM: TPQ * (prctmp/60)
                    int outTPQ = 384; //768 TPH
                    //Default TPQ rule, TPQ divisible by 60 rounded up to 960, rest is rounded up to 768.
                    int enable960TPQ = Convert.ToInt32(ticksPerQuarterNote % 60 == 0);
                    outTPQ = enable960TPQ * 960 + (1 - enable960TPQ) * 768;
                    //Special TPQ rule, for TPQs greater than 768, divisible by 128:
                    if(ticksPerQuarterNote%128 == 0 && ticksPerQuarterNote>768){
                        outTPQ = 1024;
                    }

                    //Console Separator
                    Console.WriteLine("[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]\n");
                    
                    //Counting Tracks, Notes, Tempo Events, Control Changes, Program Changes, Bitch Pends
                    int trc = 0; int nc = 0; int tec = 0; int ccc = 0; int pcc = 0; int pbc = 0;
                    
                    List<TempoEvent> tempoEvents = new List<TempoEvent>();
                    tempoEvents.Add(new TempoEvent(60000000/prctmp, 0)); //Assumes Tempo is 120 BPM, converts to seconds at 120 BPM

                    MidiEvent[][] midiEvents = new MidiEvent[mid.Events.Count()][];

                    for (int i = 0; i < midiEvents.Length; i++)
                    {
                        midiEvents[i] = mid.Events.ElementAt(i).ToArray();
                    }

                    List<AranaraN> notes = new List<AranaraN>();

                    foreach (MidiEvent m in midiEvents[0])
                    {
                        try
                        {
                            if (m is TempoEvent)
                            {
                                if (tempoEvents.Last().MicrosecondsPerQuarterNote != ((TempoEvent)m).MicrosecondsPerQuarterNote)
                                    tempoEvents.Add(m as TempoEvent);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[File {fileCount}] Error reading tempos:\n{e.Message}");
                        }
                    }

                    //Iterate through every note press in the file
                    for (int i = 0; i < midiEvents.Length; i++)
                    {
                        //Track Header
                        notes.Add(new AranaraN("TR",0,0,0,0,i,outTPQ));
                        trc++;

                        //Assume no tempo events in track 0. Default tempo is 120 (0x07A120 ms per beat)
                        if (i == 0){
                            notes.Add(new AranaraN("TE",0,0,0,0,500000,outTPQ));
                            Console.WriteLine($"[File {fileCount}] Missing Tempos, added default Tempo.\n");
                        }

                        int currentTempoIndex = 0;

                        foreach (MidiEvent midiEvent in midiEvents[i])
                        {
                            try
                            {
                                if (currentTempoIndex < tempoEvents.Count - 1)
                                {
                                    while (tempoEvents[currentTempoIndex + 1].AbsoluteTime < midiEvent.AbsoluteTime)
                                    {
                                        if (currentTempoIndex < tempoEvents.Count - 1)
                                            break;

                                        currentTempoIndex++;
                                    }
                                }

                                //Test: Switch Cases
                                double timeInSeconds;
                                double lengthInSeconds;
                                switch(midiEvent.CommandCode)
                                {
                                    case MidiCommandCode.ControlChange:
                                        ControlChangeEvent midicc = midiEvent as ControlChangeEvent;
                                        timeInSeconds = AranaraN.ToSeconds(midicc.AbsoluteTime, tempoEvents[currentTempoIndex], ticksPerQuarterNote);

                                        //Add this control change
                                        notes.Add(new AranaraN("CC",(int)midicc.Controller,(int)midicc.ControllerValue,midicc.Channel%16,timeInSeconds,0,outTPQ));
                                        ccc++;
                                    break;
                                    case MidiCommandCode.PatchChange:
                                        PatchChangeEvent midipc = midiEvent as PatchChangeEvent;

                                        timeInSeconds = AranaraN.ToSeconds(midipc.AbsoluteTime, tempoEvents[currentTempoIndex], ticksPerQuarterNote);
                                        
                                        //Add this instrument change
                                        notes.Add(new AranaraN("PC",((short)midipc.Patch),0,midipc.Channel%16,timeInSeconds,0,outTPQ));
                                        pcc++;
                                    break;
                                    case MidiCommandCode.NoteOn:
                                        NoteOnEvent note = midiEvent as NoteOnEvent;

                                        //If not an off note
                                        if (note.Velocity != 0)
                                        {
                                            timeInSeconds = AranaraN.ToSeconds(note.AbsoluteTime, tempoEvents[currentTempoIndex], ticksPerQuarterNote);
                                            lengthInSeconds = AranaraN.ToSeconds(note.NoteLength, tempoEvents[currentTempoIndex], ticksPerQuarterNote);
                                           
                                            //Add this note
                                            notes.Add(new AranaraN("N",note.NoteNumber,(int)note.Velocity,note.Channel%16,timeInSeconds,lengthInSeconds,outTPQ));
                                            nc++; 
                                        }
                                    break;
                                    default:
                                        //Tempo Event Detection
                                        if (midiEvent is TempoEvent tempo)
                                        {
                                            timeInSeconds = AranaraN.ToSeconds(tempo.AbsoluteTime, tempoEvents[currentTempoIndex], ticksPerQuarterNote);
                                            //Add Tempo Event
                                            notes.Add(new AranaraN("TE",0,0,0,timeInSeconds,60000000/tempo.Tempo,outTPQ));
                                            tec++;
                                        }

                                        //Pitch Bend Event Detection
                                        if (midiEvent is PitchWheelChangeEvent pitchBend)
                                        {                                          
                                            timeInSeconds = AranaraN.ToSeconds(pitchBend.AbsoluteTime, tempoEvents[currentTempoIndex], ticksPerQuarterNote);
                                            notes.Add(new AranaraN("PB",(int)pitchBend.Pitch,0,pitchBend.Channel,timeInSeconds,0,outTPQ));
                                            pbc++;
                                        }
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error reading note:\n{e.Message}");
                            }
                        }
                    }

                    //Aranara MIDI File Parser
                    AranaraN[] events = notes.ToArray();

                    char separateChar = '|'; //Only used for initialising file header.
                    string header = "[Faelei]█";
                    StringBuilder info = new StringBuilder($"{header}{(Path.GetFileNameWithoutExtension(path) + $"{":"}{outTPQ}").Replace(separateChar.ToString(), "")}{separateChar}");
                    

                    foreach (AranaraN n in events)
                    {
                        info.Append(n.hex_type);
                        info.Append(n.hex_note);
                        info.Append(n.hex_vel);
                        info.Append(n.hex_ch);
                        info.Append(n.hex_value);
                        info.Append(n.hex_time);
                        info.Append(n.hex_len);
                    }
                    //Write to a text file
                    File.WriteAllText(output, info.ToString());

                    //Shows Conversion Statistics
                    Console.WriteLine($"[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]=-=-=-=-=-=-=-=[]\n\nOutput FaeleiMIDI Properties [File {fileCount}]\n\nName: {Path.GetFileNameWithoutExtension(path)}\nTracks: {trc}\nNotes: {nc}\nTempo Events: {tec}\nProgram Change Events: {pcc}\nControl Change Events: {ccc}\nPitch Bend Events: {pbc}\nTotal Events: {nc + tec + pcc + ccc + pbc}\nOriginal PPQ: {ticksPerQuarterNote}\nConverted PPQ: {outTPQ}.\n");
                    return info.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[File {fileCount}] Could not process file {file}\n{e.Message}\nPress enter to continue");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine($"[File {fileCount}] File {file} does not exist.\nPress enter to continue");
                Console.ReadLine();
            }
            return "";
        }
    }
}