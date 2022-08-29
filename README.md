<img src="https://media.discordapp.net/attachments/417281262085210112/1013842879715749999/140818-F-PO994-258-scaled.jpg?width=1440&height=450">

# Fire Support

BepInEx mod for [SPT-AKI](https://hub.sp-tarkov.com/files/file/6-spt-aki/) emulator that will add insurgency-style fire support options into Escape From Tarkov.

This mod is tied to the in-game [rangefinder](https://escapefromtarkov.fandom.com/wiki/Vortex_Ranger_1500_rangefinder), so make sure it's in your inventory before you go into a raid. After you enter location, you will hear a radio message saying something like support is now available. Currently only A-10 autocannon strafe is implemented but I have plans to extend it later.

Double tap `Y` key (by default) to open gestures menu where you should notice new radial menu with available requests, their respective amounts and timer appearing after request in the center circle.

<details> 
  <summary>Radial Menu</summary>
   <img src="https://media.discordapp.net/attachments/417281262085210112/1013870628366987334/radialmenu.png?width=256&height=256">
</details>

After you select support option, by clicking on it with `LMB`, vertical spotter mark will appear. Move your mouse around to choose position and then press `LMB` again. If you want to cancel request, you can do that by hitting `Left ALT` and `RMB`. Also, if your spotter can't hit any surface (e.g. you pointing into the sky) notice message will appear.

<details> 
  <summary>Vertical Spotter</summary>
   <img src="https://media.discordapp.net/attachments/417281262085210112/1013916622681022526/spotterVertical.gif?width=700&height=256">
</details>

Autocannon strafe implies that there is a heading of where to shoot, so after you confirmed position, horizontal spotter will appear allowing you to choose direction by moving your mouse left or right. At this point you still can cancel request by pressing `Left ALT` and `RMB`, or, if you wish to confirm, press `LMB` once again.

<details> 
  <summary>Horizontal Spotter</summary>
   <img src="https://media.discordapp.net/attachments/417281262085210112/1013916623188529162/spotterHorizontal.gif?width=700&height=256">
</details>

After confirmation, you will hear a radio message from station about your support request and a short time later the A-10 will strike at the selected position and direction in a rectangular pattern. The kill zone lies somewhere between 20 to 40 metres in each direction from the horizontal spotter.

Here's video example:

<details> 
  <summary>YouTube video</summary>
   <a href="https://www.youtube.com/watch?v=el2CoSHbSK4"><img src="https://media.discordapp.net/attachments/417281262085210112/1013944435077296238/unknown.png?width=560&height=315"></a>
</details>

### How to install

1. Download the latest release here: [link](https://github.com/SamSWAT911/FireSupport/releases) -OR- build from source (instructions below)
2. Extract the zip file and drop the whole folder `SamSWAT.FireSupport` into `BepInEx/plugins/` directory.

### Requirements

- Visual Studio 2019 (.NET desktop workload)
- .NET Framework 4.7.2

### How to build from source

1. Download/clone this repository
2. VisualStudio > File > Open solution > `SamSWAT.FireSupport.sln`
3. Change references to your ones from EFT `Managed` folder
4. VisualStudio > Build > Rebuild solution
5. `SamSWAT.FireSupport.dll` should appear in `bin\Debug` directory
6. Copy `SamSWAT.FireSupport.dll` into `mod/SamSWAT.FireSupport` folder
7. Extract `assets.7z` with `here` option
8. Delete archive after extraction
9. Copy whole `SamSWAT.FireSupport` folder into `BepInEx/plugins/`
