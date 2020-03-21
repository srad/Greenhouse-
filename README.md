# Greenhouse++

## Overview

This project was developed as part the the course __System Engineering Meets Life Sciences__ at the Goethe University Frankfurt.

The goal of the course was to design a system which combines System Engineering practices with Cognitive Systems to make predictions about system with uncertain behavior/probability distributions.

The project report can be found here: https://github.com/srad/Greenhouse-Report

As part of the course this project has been implemented: An automated greenhouse `yield estimation` system for tomato plants - only based on camera pictures taken from the plants.

One part of the design of the computational pipeline was
1. Gather real world probability distributions
1. Design a computational pipeline to implemnt this goal.

## Quickstart

The CLI and WebAPI run cross-platform on .NET Core 3.x, but the WPF of course not.

### WPF

Just open the WPF project in Visual C# 2019

### Web API

```bash
git clone https://github.com/srad/GreenhousePlusPlus.git
cd GreenhousePlusPlus/GreenhousePlusPlus.WebAPI
dotnet restore
dotnet run
```

### CLI

```bash
git clone https://github.com/srad/GreenhousePlusPlus.git
cd GreenhousePlusPlus/GreenhousePlusPlus.CLI
dotnet restore
dotnet run /home/myhome/plant.jpg
```

#### Test the API from shell

Upload image:

```bash
curl -F "file=@/root/src/plant.jpg" localhost:5100/api/images

[{"element":"original","name":"7f974aa3-54ea-4297-be26-1153418600bb.jpg","path":"/Static/Images/Original/7f974aa3-54ea-4297-be26-1153418600bb.jpg"},{"element":"blur","name":"blur_7f974aa3-54ea-4297-be26-1153418600bb.png",...]
```

Get all images on the server:
 
```bash
curl -X GET localhost:5100/api/images

[{"path":"/Static/Images/Thumbs/7f974aa3-54ea-4297-be26-1153418600bb.jpg","name":"7f974aa3-54ea-4297-be26-1153418600bb.jpg"}]
```

## Implementation

This Visual Studio project is split into mutiple C# projects as the names indicate.
1. `Core` contains the Library which is build for .NET Core 3.x and .NET Framework 4.7.x
1. `WebAPI` provides a .NET Core 3.x web API implementation to consume the library functionalities.
1. `WPF` is an WPF Windows desktop application implementation in Visual C# 2019 which uses the same library as the `WebAPI`.
1. A web frontend implementation is available which consumes the web API: https://github.com/srad/GreenhousePlusPlus.App

## Algorithm

As part of the computational pipeline the `Core` client does:
1. Image segmentation and create from these information the covered leaf area of a plant per ground unit.
1. It gathers each segment's color distributions. This distributions can then be used either for simulation to generate realistic models or to increase the system's robustness.
1. Applies a image processing pipeline to find the longest vertical edge - which here is the plant's pole and is used for height estimation. This is a series of filter and algorithms.

## Screenshots

![](https://raw.githubusercontent.com/srad/GreenhousePP/master/Docs/Images/ui3.jpg)

## Platforms

A web-app which is implemented in Vue can provide an web-based responsive UI for this project, see: https://github.com/srad/GreenhousePlusPlus.App
Since the core is implemented in .NET Core it can run on Linux, MacOS and Windows.

## License 

MIT

## Todo

The computational pipeline has (as visible) paramters which are not fine tuned yet. The next step would be to implement a learning cycle to fine-tune those parameters in order to approximate the optimal values.

