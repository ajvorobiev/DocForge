# DocForge

## Introduction
**DocForge** is a class documentation tool for Arma series of game configuration files. It parses config.cpp files and documents class containment, inheritance and selected properties. You can see the first example of this tool's output at http://class.rhsmods.org .

There are a few filters that you can set up concerning the classes and properties you want to include into the output. The final output is in the form of static `html` pages that use Google chart API to draw inheritance graphs.

## Usage

As input the program takes a folder that contains one or multiple `config.cpp` files in it. The main advantage is that you can point to a large project folder where you have multiple folders with configs that become multiple pbos later. Click on the `...` button to select that folder.

You can use the `Save` and `Load` buttons to save and load (try loading `settings\DocForgeSettings1.xml` to get a some filter settings already loaded in) all your configuration settings for a certain project.

Press `Parse` to parse the initial folder. This will create the initial merged model of the parsed classes. Once that is done you should see the tree appear in the left-most treeview. Here you can browse the containment structure and in general just check if everything looks right.

### Filtering

Before you can generate output you need to filter the model. Even if you dont intend to filter anything (leaving all the filter fields blank) you still need to press the `Filter` button. But most of the time you will indeed want to filter as configs contain a lot of classes and properties that you absolutely do not need in your documentation.

The first filter list is a comma seperated list of top level includes. A top level class is a config root class. These are usually BIS defined classes like `CfgVehicles` or `CfgWeapons`.

The second list defines the classes within (recursively also) these top level classes that you want to **exclude**. Again comma seperated. All classes named here and all their children classes and properties will be excluded from the output.

The third list defines the parameter names that you want to include, like `weapons[]` and `scope`. They will appear as a table in the class description page.

Once all filters defined, use the `Filer` button to filter the model. The second and third treeviews should now contain the filtered containment and inheritance trees respectively. You can check your model in the second view. The inheritance tree is pretty useless at the moment.

### Generating

Select an output folder and press `Generate`. In the output folder find `index.html` to view the results. Internet is required for it to look right and for the charts to work.