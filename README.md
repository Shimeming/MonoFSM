## Overview

MonoFSM is a comprehensive Finite State Machine (FSM) framework for Unity, designed to simplify the implementation of complex game behaviors and logic. The framework provides a visual and modular approach to designing state machines, making it easier to create, debug, and maintain game systems.
[Nine Sols](https://store.steampowered.com/app/1809540/Nine_Sols/) is the first project to use this framework, and it has been developed and tested in the context of that game.
## Pre-requirement Dependencies
### Paid Tools
  * [Odin-Inspector](https://odininspector.com/)
### Free Tools
  * Unity Official Package
    * Unity.Addressable
    * Unity.Timeline
  * ThirdParty Tools
    * [UniTask](https://github.com/Cysharp/UniTask)
    * [ZString](https://github.com/Cysharp/ZString)
    * [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween)
### Included in Project (with modification)
* Auto Attribute
* GuidManager
* System.Runtime.CompilerServices.Unsafe

# Installation
* Just use: Install through Unity Package Manager with git url
* To Contribute: use "git submodule" to include this module into your project, and add as local package through Unity package manager

## Why Use MonoFSM?

1. **Seamless Integration in Unity's Scene Hierarchy**  
   MonoFSM appears directly in Unity’s Scene hierarchy, allowing roles like *programmers*, *designers*, and *artists* to work on the same objects naturally. This fosters organic information sharing within the project and reduces the need for extensive documentation.  

2. **Prefab-Based Extensibility and Reusability**  
   By leveraging Unity’s Prefab and Prefab Variant system, MonoFSM overcomes the traditional FSM limitations of poor reusability and difficulty in expansion, making it straightforward to build and extend state machines.  

3. **Intent-Driven Dependency Injection (DI)**  
   Using GameObjects as the foundation allows for clear and intuitive dependency injection. Object activation and deactivation become explicit expressions of intent, which can be further interpreted and adapted later.  

4. **Tight Integration with Unity’s Native Tools**  
   Because MonoFSM is built on GameObjects, it naturally supports Unity’s Animation Clips and Timeline. This enables designers to create timeline-driven state machines with fine-grained, time-based control suitable for level design and gameplay micro-adjustments.


## Core Features

### Editor Tools
### todo

## Getting Started
### todo

## Example Usage
### todo
