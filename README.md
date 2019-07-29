![alt text](media/simple-wix.png "Simple Wix")

# Simple Wix


<p align="center">
  <i>~~~~</i>
</p>
<p align="center">
  <i>Come home to the impossible flavour of your own completion. Come home to Simple WiX. </i>
</p>

<p align="center">
  <i>~~~~</i>
</p>

Simple WiX is an open source command line utility to automatically create .msi installers using the open source WiX toolset. If you want to generate a Windows installer (.msi) file that just needs to copy files onto the user's machine, update them with new versions, and delete a settings folder or two on uninstall, then Simple Wix is the right tool for you. 

<p align="center">
<img src=media/installer.png alt="Simple Wix"/>
</p>

This project was born out of the frustrations endured when trying to create a simple installer for some plugins that I was developing. When using frameworks such as InnoSetup I found that I would often run into inexplicable permissions issues that were relatively opaque and difficult to debug. I found that the [WiX Toolset](https://wixtoolset.org/) worked reliably, was transparent and easy to debug, and had a lot of documentation and developer support, but the process of writing WiX files is rather ... onerous, to say the least, so this tool is designed to do it all for you. 

## Getting Started

To learn how to use this tool to create an installer for your project, see the [Getting Started](docs/gettingstarted.md) page in the Wiki.

To get this repo up and running for development / contribution purposes, just clone the repository and build the solution. To test the full process you'll also want to download and install the latest [WiX Toolset](https://wixtoolset.org/releases/) (3.11 at time of writing). See deployment for notes on how to deploy the project on a live system.

### Prerequisites

* [WiX Toolset](https://wixtoolset.org/releases/)


### Installing

A step by step series of examples that tell you how to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [commandlineparser](https://github.com/commandlineparser/commandline) - for making sense of all those options.
* [Newtonsoft JSON.NET](https://www.newtonsoft.com/json) - Json stuff
* [Costura Fody](https://github.com/Fody/Costura) - Weaving a single exe
* [Paint.NET](https://www.getpaint.net/) - Everything media related

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/dialog-development/simple-wix/tags). 

## Authors

* **Jason Masters** - *Initial work* - [m-sterspace](https://github.com/m-sterspace)

See also the list of [contributors](https://github.com/dialog-development/simple-wix/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc

