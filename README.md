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

## Installation

### Prerequisites

Before installing MonoFSM, you must install the required dependencies:

1. **Install Odin Inspector** (Paid)
   - Purchase and install [Odin Inspector](https://odininspector.com/) from the Asset Store

2. **Install Unity Official Packages**
   - Open Package Manager → Unity Registry
   - Install `Addressables` package
   - Install `Timeline` package

3. **Install Third-Party Packages**
   - **UniTask**: Add `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask` via Package Manager
   - **ZString**: Add `https://github.com/Cysharp/ZString.git?path=src/ZString/Assets/Scripts/ZString` via Package Manager  
   - **PrimeTween**: Add `https://github.com/KyryloKuzyk/PrimeTween.git` via Package Manager

### Install MonoFSM

* **Quick Setup**: Install through Unity Package Manager with git url
* **For Contributors**: Use "git submodule" to include this module into your project, and add as local package through Unity Package Manager

## Why Use MonoFSM?

1. **Seamless Integration in Unity's Scene Hierarchy**
   
   MonoFSM appears directly in Unity's Scene hierarchy, allowing roles like *programmers*, *designers*, and *artists* to work on the same objects naturally. This fosters organic information sharing within the project and reduces the need for extensive documentation.

2. **Prefab-Based Extensibility and Reusability**
   
   By leveraging Unity's Prefab and Prefab Variant system, MonoFSM overcomes the traditional FSM limitations of poor reusability and difficulty in expansion, making it straightforward to build and extend state machines.

3. **Intent-Driven Dependency Injection (DI)**
   
   Using GameObjects as the foundation allows for clear and intuitive dependency injection. Object activation and deactivation become explicit expressions of intent, which can be further interpreted and adapted later.

4. **Tight Integration with Unity's Native Tools**
   
   Because MonoFSM is built on GameObjects, it naturally supports Unity's Animation Clips and Timeline. This enables designers to create timeline-driven state machines with fine-grained, time-based control suitable for level design and gameplay micro-adjustments.

## Core Features

### Visual State Management

MonoFSM integrates directly into Unity's Scene hierarchy, making state machines visible and editable as GameObjects.

- **GameObject-based States**: States appear as GameObjects in the hierarchy, enabling intuitive drag-and-drop design
- **Complete Lifecycle**: Full support for OnStateEnter, OnStateUpdate, OnStateExit, and OnStateFinally methods
- **Safe Transitions**: Provides both safe and overwrite transition modes to prevent state conflicts

### Variable System

Powerful variable management with cross-state data sharing and real-time monitoring capabilities.

- **Typed Variables**: Built-in support for VarBool, VarInt, VarFloat, VarString, VarTransform, and custom types
- **Dynamic Binding**: Variable binding system with change listeners for reactive programming
- **Tag Organization**: VariableTag system for efficient variable categorization and lookup

### Action & Condition Framework

Flexible action execution system with conditional logic for complex gameplay behaviors.

- **Conditional Execution**: Actions execute only when specified conditions are met
- **Async Support**: Integrated with UniTask for delayed execution and non-blocking operations  
- **Extensible Architecture**: Easy custom action creation through AbstractStateAction inheritance

### Object Pool Management

High-performance object pooling system with automated lifecycle management.

- **PoolBank Automation**: Scene-level pool configuration with automatic prewarming data generation
- **Memory Optimization**: Smart pool sizing to reduce memory fragmentation and GC pressure
- **Transform Management**: Automatic capture and restoration of object Transform states

### Unity Native Tools Integration

Deep integration with Unity's built-in systems unlocks new possibilities for expressive state machine design.

- **Animation Clips**: Direct state-to-animation mapping with automatic playback control and transition handling
- **Timeline Integration**: Timeline-driven state machines for cinematic sequences and complex scripted events
- **Prefab Variants**: Leverage Unity's Prefab Variant system to create reusable state machine templates with inheritance

### Editor Tools Integration

Comprehensive editor toolset for enhanced development workflow and debugging experience.

- **Visual Debugging**: Real-time state machine monitoring with custom hierarchy display
- **Auto Attributes**: Automatic component reference resolution and hierarchy relationship management
- **Custom Drawers**: Rich Inspector experience with specialized property drawers and selectors

## Getting Started: SimpleDoor Example

This tutorial walks you through creating a simple door FSM that demonstrates MonoFSM's core concepts.

### Scene Preparation

First, add the `SinglePlayer World Simulator.prefab` to your scene (located in the project).
*MonoFSM's lifecycle is controlled through this component*

![World Simulator](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/WorldSimulator.png)

### Create New Prefab

1. Right-click on `Packages/MonoFSM/0_MonoFSM_Example_Module/General FSM.prefab`
2. Select **[Prefab Variant]**

![Create Prefab Variant](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/CreatePrefabVariant.png)

3. Rename the generated `General FSM Variant` to your desired object name, for example:
   `General FSM Variant - Door.prefab`
4. Move it to your project's appropriate location, such as:
   `Assets/FSMs/Puzzles/General FSM Variant - Door.prefab`

### Configure States

Edit the `[General FSM Variant - Door.prefab]`:

**[States]**
Copy and rename existing states to create: `[State] Closed`, `[State] Opening`, `[State] Opened`, `[State] Closing`

![Create State](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/CreateState.png)

### Setup Variables

**[Variables]**
1. Click the Variables node
2. In Inspector → AddChild → Add VarBool named `[VarBool] Should Open`

![Variables](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/Variables.png)

### LogicRoot Setup

**[LogicRoot]**
Add SpriteRenderer and Collider2D components to represent the door's visual and physical properties.

![Door Physics](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/DoorPhysics.png)

### Animator Configuration

**[Animator]**
Create four Animation Clips corresponding to each state. Remember to turn off LoopTime on the Animation Clips (unless you really need looping).

![Door Animations](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/PrefabExampleDoorAnimations.mov)

### Bind States to Animations

**[State & Animation Binding]**
Connect each state with its corresponding animation.

![State and Animations](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/StateAndAnimations.png)

### Configure Transitions

**[State & Transition Binding]**
- Define that Opening animation completion transitions to Opened state
- Define that Closing animation completion transitions to Closed state

![Animation Done Transition](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/AnimationDoneTransition.png)

### Setup Conditional Transitions

**[State, Variable & Transition Binding]**
Configure transitions and their corresponding conditions using the Should Open variable.

![Transition and Condition](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/TransitionAndCondition.png)

### Final Result

Your SimpleDoor FSM is now ready! The door will respond to the Should Open variable changes and smoothly transition between states.

![Simple Door Result](MonoFSM/0_MonoFSM_Example_Module/Document/DocumentAssets/SimpleDoorResult.mov)

## Example Usage
### todo
