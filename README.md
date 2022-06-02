# Endless runner
My code from game https://hrober.itch.io/escape-the-simulation <br>
Game was made on Brackeys Game Jam 2022.1 in week.

Features:
- Procedural world generation
- Skill system
- WebGL save system
- Player controller 2d


## Interesting parts

Game master - manages states of game:<br>
https://github.com/Hrober0/Endless-runner/blob/main/Scripts/GameMaster.cs

Audio manager - base on SO, Supporting smooth music changes, playing SFXs in various configurations:<br>
https://github.com/Hrober0/Endless-runner/blob/main/Scripts/Audio/AudioManager.cs

Map controler - selecting new stages of the game according to specific rules and removing old parts of the map:<br>
https://github.com/Hrober0/Endless-runner/blob/main/Scripts/Gameplay/Map/StageManager.cs

Skill manager - Keep information about having and not having skills,  save and load skills:<br>
https://github.com/Hrober0/Endless-runner/blob/main/Scripts/Skills/SkillManager.cs


## Used Technologies

#### Unity and C#
- UI toolkits
- Scriptable objects
- TileMap
- OpenGL
