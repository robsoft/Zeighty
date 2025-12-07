# Zeighty
("Zay-tee")

## C#/MonoGame-based Gameboy Emulator

last updated 5 Dec 2025  

next steps: 
1) Implement Serial, Timer, Interrupts, DMA as per video  
https://www.youtube.com/watch?v=hrK9nc13vxo
2) More PPU work
https://www.youtube.com/watch?v=--Y0J0LRbd0


### In-flight  
- implement remaining debugger keyboard controls (jump to locations, set breakpoints, edit ram values)   
- visualise VRAM - can see tile data but not map etc 


### To-do  
- run some standard test roms to verify CPU emulation  
- implement additional debugging features
- start PPU emulation  
- interrupt handling  
- keyboard/joypad handling  
- audio emulation  
- cartridge loading (MBC1, MBC2, etc)  
- save states  
- implement timing/delays to match real hardware speed  
- optimize performance  
- build a simple UI for loading roms and displaying emulator state  
- write documentation and usage instructions  


### Main-update-draw loop 'states' I will need 
- Basic 'run emulator/debugger' (done)
- File requester to load roms/states
- File requester to save states
- Settings menu (audio/video/input)
- Edit value at a memory view address
- Edit memory view address 
- Edit breakpoint at a memory address



### Done  
- Z80 CPU Emulation  
- single-step/debugger framework  
- moved the console app into MonoGame  

