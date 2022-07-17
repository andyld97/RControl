# Welcome to RControl

RControl is a little `.NET`-Console-App which (CLI) helps you to control your `relay-card` from `command-line` or even from other applications!
Since this app is running on `.NET Core` it can be used on every platform, but you have to install `.NET` first! In the past I used it with `mono` to control my `relay-card` wit a linux server, but this isn't necessary anymore!

# Based On 
https://github.com/hb9fxq/RelayCard

# ToDo
- Also provide this as a library to simplify the integration for `.NET-Apps`

# Example
![Example screenshot](https://user-images.githubusercontent.com/10423894/179399063-281553ab-76f7-4472-ab51-2083b61ca7e3.png)

# Documentation for RelayControl.exe v1.0.0.2
## Basic Commands
| Command                | Description                                                            |
|------------------------|------------------------------------------------------------------------|
| -h                     | Shows the documentation                                                |
| -p                     | Port: Default port is /dev/ttyUSB0 for usage on Linux with Mono.       |
| -s                     | Switch all relays off on each card                                     |
| -s -c 1...255          | Switch all relays off on Card X                                        |
| -t -r 1...8            | Handle Relay r on Card1 as a Taster (Switch on and after a delay off)  |
| -t -c 1...255 -r 1...8 | Handle Relay r on Card c as a Taster (Switch on and after a delay off) |

## Simple swichting (must be always the first parameter)

| Command                | Description                                                                           |
|------------------------|---------------------------------------------------------------------------------------|
| AAAAAAAA               | A bit sequence which will be sent to Card 1                                           |
| X,AAAAAAAA             | A bit sequence which will be sent to Card X 1 <= X <= 255                             |
| 0,AAAAAAAA             | Broadcast: This command will be sent and exceuted by each and every card              |
| AAAAAAAABBBBBBBB       | A bit sequence which will be splitted and each byte send to card x. x = 1; increments |
                        
 ## Getting state    
| Command                | Description                                                                           |
|------------------------|---------------------------------------------------------------------------------------|
| -a                     | This command returns the state of all cards like AAAAAAAABBBBBBBB...                  |
| 1 ... 255              | This command returns the state of the selected card                                   |
|                        | No command returns always the state of card 1                                         |
