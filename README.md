# Greenhouse++

This software is a tool used within a system engineering course. The aim of the course was to integrate within system engineering design congnitive system approaches.

One very small step during that journey was image processing to gather real world probability distributions. The specific system  was designed for an automated greenhouse yield estimation system for tomato plants - only based on camera pictures taken from the plants.

As part of the computational pipeline this C# WPF client does firstly image segmentation and create from these information the covered leaf area of a plant per ground unit. Secondly, it gathers each segment's color distributions. This distributions can then be used either for simulation to generate realistic models or to increase the system's robustness.

![](https://raw.githubusercontent.com/srad/GreenhousePP/master/Docs/Images/ui0.png)
