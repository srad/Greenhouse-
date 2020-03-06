# Greenhouse++

This software is a tool used within a system engineering course. The aim of the course was to integrate within system engineering design congnitive system approaches.

One very small step during that journey was image processing to gather real world probability distributions. The specific system  was designed for an automated greenhouse yield estimation system for tomato plants - only based on camera pictures taken from the plants.

As part of the computational pipeline this C# WPF client does:

1. Image segmentation and create from these information the covered leaf area of a plant per ground unit.
1. It gathers each segment's color distributions. This distributions can then be used either for simulation to generate realistic models or to increase the system's robustness.
1. Applies a image processing pipeline to find the longest vertical edge - which here is the plant's pole and is used for height estimation.

![](https://raw.githubusercontent.com/srad/GreenhousePP/master/Docs/Images/ui3.jpg)

This pipeline has (as visible) paramters which are not finetubed yet through any learning, but would be the next step
