# ZusiTcpInterface
An implementation of the Zusi 3 network protocol for .NET

## Synopsis

The ZusiTcpInterface library is an implementation of the Zusi 3 network protocol with an emphasis on providing a simple, reliable API. The aim is to create a library that allows users to interact with the simulator with a minimum of setup, helpful error messages and type-safety. The library is modularised so that users can choose to use one of the provided beginner-friendly front-ends or to implement their own.

## Code Example

### Basic Setup
```CS
// Create backend with default path to config file 
var connectionContainer = new ConnectionContainer();

// Create frontend. This one uses thread marshalling to automatically pass incoming data to the GUI
var dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionContainer.ReceivedDataChunks, SynchronizationContext.Current);
```

### Creating a list of data to request from the simulator
```CS
// All information about available data, their types, units etc is accessible through a tree of descriptors.
var velocityDescriptor = _connectionContainer.Descriptors.AttributeDescriptors["Geschwindigkeit"];
var gearboxPilotLightDescriptor = _connectionContainer.Descriptors.AttributeDescriptors["LM Getriebe"];
var sifaStatusDescriptor = _connectionContainer.Descriptors.NodeDescriptors["Status Sifa"];

// Collect the IDs of the data required into a list
var neededData = new List<short> { velocityDescriptor.Id, gearboxPilotLightDescriptor.Id, sifaStatusDescriptor.Id };
```

### Wiring up the frontend to callbacks
```CS
  ... (basic setup) ...
  
  // Register callbacks. Addresses have to be hand-written at the moment - see CabInfoTypes.xml for details
  // Each callback registration applies to one bit of data, e.g. the gearbox indicator light or current velocity.
  dataReceiver.RegisterCallbackFor<bool>(new CabInfoAddress(0x1A), OnGearboxPilotLightReceived);
  dataReceiver.RegisterCallbackFor<bool>(new CabInfoAddress(0x64, 0x02), OnSifaPilotLightReceived);
  dataReceiver.RegisterCallbackFor<float>(new CabInfoAddress(0x01), OnVelocityReceived);
  ...

// Callbacks
private void OnGearboxPilotLightReceived(DataChunk<bool> dataChunk)
{
  lblGearboxIndicator.Text = dataChunk.Payload.ToString();
}

private void OnSifaPilotLightReceived(DataChunk<bool> dataChunk)
{
  lblSifaStatus.Text = dataChunk.Payload.ToString();
}

private void OnVelocityReceived(DataChunk<float> dataChunk)
{
  lblVelocity.Text = String.Format("{0:F1}", dataChunk.Payload * 3.6f);
}

```

### Establishing a connection
```CS
// Establish the connection.
connectionContainer.Connect("My Little Example Application", "1.0.0.0", neededData);

// Wait for data to "magically" appear in the callbacks :)
```

## Motivation
This project was originally created from the desire to write auxiliary tools for the then-current version 2.4 of Zusi. At the time there was no protocol implementation available for the original author's favourite platform (.Net, obviously), so he began to write his own. It quickly became clear that such a protocol implementation should be reusable across projects, and that it would help the community if others were able to integrate it into their projects as well. Thus the library was published, first as closed source, later opened up under the Lesser GNU Public License.

After the official release of Zusi 3 in March 2016, which uses a completely new protocol, the library was rewritten from scratch, this time in professional quality.

## Installation
The library was developed in Visual Studio 2013 and 2015. C# 4.0 and the .Net Framework 4.5 were chosen as targets to ensure compatibility with legacy applications and toolchains.

Open the [main solution file](ZusiTcpInterface.sln) in Visual Studio to build the library, Unit Tests and example projects.
The library itself is contained in its entirety within the [ZusiTcpInterface](ZusiTcpInterface/) folder. There are no additional dependencies that need to be installed.

Demo applications have been provided as examples and starting points for own projects.

## API Reference
At this point no formal documentation exists. Please use the examples provided and unit tests as a reference.

## Tests
The Unit Test project uses the Microsoft Test framework. The test project (ZusiTcpInterfaceTests) is part of the main solution and can be compiled and run from Visual Studio.

## Contributors
Feel free to fork and use this library to your heart's content. Please report any issues, feature requests and general feedback through the GitHub issue tracker or the Zusi forums.

If you wish to contact the author directly, please send an [email](clonkman@gmx.de) or send a forum message to Andreas Karg on the Zusi forums.

You are invited to contribute to the project. I will review all pull requests I receive on GitHub and provide constructive feedback on them. If they match (or preferably improve on) the quality standard and provide additional value, I will merge them in at my own discretion.

## License
The project is licensed under the GNU Lesser General Public License v3 where not stated otherwise (see below). Please see the [license file](LICENSE.md) for details.
Some sub folders (examples and Demo Apps) contain a separate license (`legalcode.txt`, `LICENSE.md` or `.txt` etc) that overrides the main license. This is to allow their free use for any purpose and context.
