# Greenhouse++

This application was developed as part the the course __System Engineering Meets Life Sciences__ at the Goethe University Frankfurt.
The goal of the course was to design a system which combines System Engineering practices with Cognitive Systems to make predictions about system with uncertain behavior/probability distributions.

The project report can be found here: https://github.com/srad/Greenhouse-Report

As part of the course this project has been implemented: An automated greenhouse `yield estimation` system for tomato plants - only based on camera pictures taken from the plants.

One part of the design of the computational pipeline was
1. Gather real world probability distributions
1. Design a computational pipeline to implemnt this goal.

As part of the computational pipeline this C# WPF client does:
1. Image segmentation and create from these information the covered leaf area of a plant per ground unit.
1. It gathers each segment's color distributions. This distributions can then be used either for simulation to generate realistic models or to increase the system's robustness.
1. Applies a image processing pipeline to find the longest vertical edge - which here is the plant's pole and is used for height estimation.

![](https://raw.githubusercontent.com/srad/GreenhousePP/master/Docs/Images/ui3.jpg)

## Todo

The computational pipeline has (as visible) paramters which are not fine tuned yet. The next step would be to implement a learning cycle to fine-tune those parameters in order to approximate the optimal values.
