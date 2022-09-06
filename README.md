![](https://img.shields.io/badge/Licence-Apache%202.0-green.svg)[![NuGet](https://img.shields.io/badge/NuGet-Databridges.Sio.Client.Lib-%23004880)](https://www.nuget.org/packages/Databridges.Sio.Client.Lib)[![Target Framework](https://img.shields.io/badge/Target%20Framework-.NET%20Standard%202.0-%237014e8)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support)


# Databridges C# client Library

DataBridges makes it easy for connected devices and applications to communicate with each other in realtime in an efficient, fast, reliable and trust-safe manner. Databridges C# client library allows you to easily add realtime capabilities to your applications in record time.

## Have you registered with dataBridges?

Registering allows you to access our production grade PlayGround network. You can take dataBridges for a spin and quickly create, test your proof of concepts without needing to create a payment account.

We would love to see what you build on dataBridges platform. We are available at tech@optomate.io to answer all your queries.

https://www.databridges.io/developers.html

## Usage Overview

The following topics are covered:
- [Installation](#installation)
- [Initialization](#initialization)
- [Global Configuration](#global-configuration)
  - [Required](#required)
  - [Optional](#optional)
- [Connection](#connection)
- [Objects](#objects)
- [object:ConnectionState](#objectconnectionstate)
  - [Properties](#properties)
  - [Bind to connectionstate events](#bind-to-connectionstate-events)
  - [Functions](#functions)
- [object:Channel](#objectchannel)
  - [Subscribe to Channel](#subscribe-to-channel)
  - [Connect to Channel](#connect-to-channel)
  - [Channel Information](#channel-information)
  - [Publish to Channel](#publish-to-channel)
  - [Binding to events](#binding-to-events)
  - [System events for channel object](#system-events-for-channel-object)
- [object: rpc (Remote Procedure Call)](#object-rpc-remote-procedure-call)
  - [Connect to Server](#connect-to-server)
  - [Server Information](#server-information)
  - [Execute Remote Procedure Call](#execute-remote-procedure-call)
  - [System events for rpc object](#system-events-for-rpc-object)
- [object:Cf (Client Function)](#objectcf-client-function)
  - [Properties](#properties)
  - [System events for cf object](#system-events-for-cf-object)
- [Change Log](#change-log)
- [License](#license)

## Supported platforms

- .NET and .NET Core  -	2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0
- .NET Framework 1    -	4.6.1 2, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8


## Installation

You can use any visual studio package manager, or add reference of dbridge-client.dll.

```bash
Install-Package Databridges.Sio.Client.Lib
```

> Note : Databridges library uses socket.io for websocket protocol management.

## Initialization

```c#
using dBridges;
dBridges.dBridges dbridge = new dBridges.dBridges();
```

## Global Configuration

### Required

The following is the list of required connection properties before connecting to dataBridges network.

```c#
dbridge.auth_url = 'URL'
dbridge.appkey = 'APP_KEY'
```

You need to replace `URL` and `APP_KEY` with the actual URL and Application Key.

| Properties | Description                                                  | Exceptions                                               |
| ---------- | ------------------------------------------------------------ | -------------------------------------------------------- |
| `auth_url` | *(string)* Authentication url from  [dataBridges dashboard](https://dashboard.databridges.io/). | `source: DBLIB_CONNECT` <br />`code: INVALID_URL`        |
| `appkey`   | *(string)* Application Key from  [dataBridges dashboard](https://dashboard.databridges.io/). | `source: DBLIB_CONNECT` <br />`code: INVALID_AUTH_PARAM` |

### Optional

The following is the list of optional connection properties before connecting to dataBridges network.

```c#
dbridge.maxReconnectionRetries = 10;
dbridge.maxReconnectionDelay = 120000; 
dbridge.minReconnectionDelay = 1000 + Math.random() * 4000;
dbridge.reconnectionDelayGrowFactor = 1.3;
dbridge.minUptime = 200 ; 
dbridge.connectionTimeout = 10000;
dbridge.autoReconnect = true; 
dbridge.cf.enable = false; 
```

| Properties                    | Default                       | Description                                                  |
| ----------------------------- | ----------------------------- | ------------------------------------------------------------ |
| `maxReconnectionDelay`        | `10`                          | *(integer)* The maximum delay between two reconnection attempts in seconds. |
| `minReconnectionDelay`        | `1000 + Math.random() * 4000` | *(integer)* The initial delay before reconnection in milliseconds (affected by the `reconnectionDelayGrowFactor` value). |
| `reconnectionDelayGrowFactor` | `1.3`                         | *(float)* The randomization factor used when reconnecting (so that the clients do not reconnect at the exact same time after a server crash). |
| `minUptime`                   | `200`                         | *(integer)* Uptime before `connected` event is triggered, value in milliseconds. |
| `connectionTimeout`           | `10000`                       | *(integer)* Number of milliseconds the application will wait for a connection to be established. If it fails it will emit a `connection_error` event. |
| `maxReconnectionRetries`      | `10`                          | *(integer)* The number of reconnection attempts before giving up. |
| `autoReconnect`               | `true`                        | *(boolean*) If false, application will not attempt reconnecting. |
| `cf.enable`                   | `false`                       | *(boolean)* Enable exposing *client function* for this connection. (Check *Client Function* section for details.) |
| `access_token`                | `function`                    | *(function)* If you need custom authorization behavior for  generating signatures for private/ presence/ system channels/rpc functions, you can provide your own `access_token` function. |

## Connection

Once the properties are set, use `connect()` function to connect to dataBridges Network.

```c#
try {
    await dbridge.connect();
} catch (dBError err) {
    console.WriteLine(err);
}
```

#### Exceptions: 

| Source        | Code                         | Message                         | Description                                                  |
| ------------- | ---------------------------- | ------------------------------- | ------------------------------------------------------------ |
| DBLIB_CONNECT | INVALID_URL                  |                                 | Value of `dbridge.auth_url` is not a valid dataBridges authentication URL. |
| DBLIB_CONNECT | INVALID_AUTH_PARAM           |                                 | Value of `dbridge.appkey` is not a valid dataBridges application key. |
| DBLIB_CONNECT | INVALID_ACCESSTOKEN_FUNCTION |                                 | If *"callback function"* is not declared for authentication **only** while using  **private, presence or system** channel/RPC functions. *(Check Channel or RPC section for details.)* |
| DBLIB_CONNECT | HTTP_                        | HTTP protocol reported message. | HTTP Errors returned during authentication process. ***HTTP Error code*** will be concatenated with `HTTP_` in the `err.code`. `eg. HTTP_501` |
| DBLIB_CONNECT | INVALID_CLIENTFUNCTION       |                                 | If *"callback function"* is not declared for client function **or** `typeof()` variable defined is not a *"function"*. This is applicable only if clientFunction is enabled. *(Check Client Function section for details.)* |

#### sessionid *(string)*

```c#
Console.WriteLine(dbridge.sessionid);
```

Making a connection provides the application with a new `sessionid` that is assigned by the server. This can be used to distinguish the application's own events. A change of state might otherwise be duplicated in the application. It is also stored within the connection, and used as a token for generating signatures for private/presence/system channels/rpc functions.

#### disconnect *(function)*

To **close a connection** use disconnect function. When a connection has been closed explicitly, no automatic reconnection will happen.

```c#
dbridge.disconnect();
```

## Objects

| Object            | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| `connectionstate` | connectionstate object expose properties, functions and events to monitor and manage the health of dataBridges network connection. |
| `channel`         | channel object exposes **trust-safe** flexible Pub/Sub messaging properties, functions and events to build realtime event messaging / event driven applications at scale. |
| `rpc`             | rpc object exposes **trust-safe** properties, functions and events to provide reliable two-way messaging between multiple endpoints allowing you to build sophisticated asynchronous interactions. |
| `cf`              | CF (Client-function) object is a special purpose RPC implementation to build command and control applications. CF object exposes properties, functions and events for command and control server applications to send messages to devices and application using dataBridges library in **trust-safe manner **, build smart update configuration system and implement **trust-safe ** actions for remote and automated management. |



## object:ConnectionState

Connectionstate object expose properties, functions and events to monitor and manage the health of dataBridges network connection.

### Properties

The following is the list of connection state properties.

```c#
Console.WriteLine(dbridge.connectionstate.state);
Console.WriteLine(dbridge.connectionstate.isconnected);
Console.WriteLine(dbridge.connectionstate.rttms);
Console.WriteLine(dbridge.connectionstate.reconnect_attempt);
```

| Property                            | Description                                                  |
| ----------------------------------- | ------------------------------------------------------------ |
| `connectionstate.state`             | *(String)* Current state of dataBridges network connection . List of Return Values are detailed below. |
| `connectionstate.isconnected`       | *(Boolean)* To verify if the application is still connected to the dataBridges network. |
| `connectionstate.rttms`             | *(integer)* Latency in milliseconds between your application and the dataBridges router where your application is connected. |
| `connectionstate.reconnect_attempt` | (integer) Number of reconnection attempted as of now.        |

##### connectionstate.state

| Return Values      | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| *connecting*       | Your application is now attempting to connect to dataBridges network. |
| *connected*        | The connection to dataBridges network is open and authenticated with your `appkey`. |
| *connection_break* | Indicates a network disconnection between application and dataBridges network. The library will initiate an automatic reconnection, if the reconnection property is set as true. |
| *connect_error*    | The dataBridges network connection was previously connected and has now errored and closed. |
| *disconnected*     | The application is now disconnected from the dataBridges network. The application will than need to initiate fresh connection attempt again. |
| *reconnecting*     | Your application is now attempting to reconnect to dataBridges network as per properties set for reconnection. |
| *reconnect_error*  | Reconnection attempt has errored.                            |
| *reconnect_failed* | The application will enter reconnect_failed state when all the reconnection attempts have been exhausted unsuccessfully. The application is now disconnected from the dataBridges network. The application will than need to initiate fresh connection attempt again |
| *reconnected*      | *The application has successfully re-connected to the dataBridges network,* This state will follow `connect_error` **or** `reconnect_error`. |

### Bind to connectionstate events

Apart from retrieveing state of a dBrige connection, application can bind to connectionstate events.

You can use the following methods on connectionstate object to bind to events.

```c#
dbridge.connectionstate.bind(eventName, Action<object>);
dbridge.connectionstate.unbind(eventName);
dbridge.connectionstate.unbind();
```

`bind()` on `eventName` has callback functions to be defined where you can write your own code as per requirement.

To stop listening to events use `unbind(eventName)` function.

To stop listening to all events use `unbind()` *[without eventName]* function.

Below are library events which can be bind to receive information about dataBridges network.

#### System events for connectionstate object

```c#
Action<object> connecting;
dbridge.connectionstate.bind("connecting", connecting = (object payload) => {
    Console.WriteLine("connecting");
});

Action<object> reconnecting;
dbridge.connectionstate.bind("reconnecting", reconnecting = (object payload) => {
	Console.WriteLine("reconnecting, reconnect_attempt={0}", dbridge.connectionstate.reconnect_attempt);
});

Action<object> disconnected;
dbridge.connectionstate.bind("disconnected", disconnected = (object payload) => {
    Console.WriteLine("disconnected");
});

Action<object> reconnected;
dbridge.connectionstate.bind("reconnected", reconnected = (object payload) => {
   Console.WriteLine("reconnected");
}); 

Action<object> disconnected;
dbridge.connectionstate.bind("connection_break", reconnected = (object payload) => {
	dBError Payload =  payload as dBError
	Console.WriteLine("connection_break {0},{1},{2}", Payload.source, Payload.code, Payload.message);
});

Action<object> state_change;
dbridge.connectionstate.bind("state_change", state_change = (object payload) => {
	stateChange statechange = payload as stateChange;
	Console.WriteLine("state_change {0} , {1}", dbridge.connectionstate.state, payload);
});

Action<object> connect_error;
dbridge.connectionstate.bind("connect_error", connect_error = (object payload) => {
    dBError Payload =  payload as dBError
    Console.WriteLine("connect_error,  {0},{1},{2}", Payload.source, Payload.code, Payload.message); 
});

Action<object> reconnect_error;
dbridge.connectionstate.bind("reconnect_error", reconnect_error = (object payload) => {
    dBError Payload =  payload as dBError
    Console.WriteLine("reconnect_error {0},{1},{2}", Payload.source, Payload.code, Payload.message); 
});

Action<object> reconnect_failed;
dbridge.connectionstate.bind("reconnect_failed", reconnect_failed = (object payload) => {
	dBError Payload =  payload as dBError
	Console.WriteLine("reconnect_failed{0},{1},{2}", Payload.source, Payload.code, Payload.message); 
});

Action<object> rttpong;
dbridge.connectionstate.bind("rttpong", rttpong = (object payload) => {
   Console.WriteLine("rttpong {0} , {1}", payload, dbridge.connectionstate.rttms);
}); 

Action<object> connected;
dbridge.connectionstate.bind("connected", connected = (object payload) => {
    Console.WriteLine("dBridge Event Bind {0}", "connected");
});
```
###### payload: `(dberror object)`

```c#
{
    "source": "DBLIB_CONNECT" , 			// (string) Error source
    "code": "RECONNECT_ATTEMPT_EXCEEDED",	// (string) Error code 
    "message": "" 							// (string) Error message if applicable.
}
```

| Events             | Parameters                                         | Description                                                  |
| ------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| `connecting`       |                                                    | This event is triggered when your application is attempting to connect to dataBridges network. |
| `connected `       |                                                    | This event is triggered when connection to dataBridges network is open and authenticated with your `appkey`. |
| `connection_break` | *payload*                                          | *(dberror object)* Indicates a network disconnection between application and dataBridges network. The library will initiate an automatic reconnection, if the reconnection property is set as true. |
| `connect_error`    | *payload*                                          | *(dberror object)* This event is triggered when the dataBridges network connection was previously connected and has now errored and closed. |
| `disconnected`     |                                                    | The application is now disconnected from the dataBridges network. The application will than need to initiate fresh connection attempt again. |
| `reconnecting`     |                                                    | This event is triggered when  application is now attempting to reconnect to dataBridges network as per properties set for reconnection. |
| `reconnect_error`  | *payload*                                          | *(dberror object)* This event is triggered when reconnection attempt has errored. |
| `reconnect_failed` | *payload*                                          | *(dberror object)* reconnect_failed event is triggered when all the reconnection attempts have been exhaused unsuccessfully. The application is now disconnected from the dataBridges network. The application will than need to initiate fresh connection attempt again. |
| `reconnected`      |                                                    | This event is triggered when the connection to dataBridges network is open and reconnected after `connect_error` **or** `reconnect_error`. |
| `state_change`     | *payload* with `payload.previous, payload.current` | *(dict)* This event is triggered whenever there is any state changes in dataBridges network connection. Payload will have previous and current state of connection. |
| `rttpong`          | `payload`                                          | *(integer)* In Response to `rttping()` function call to dataBridges network, payload has latency in milliseconds between your application and the dataBridges router where your application is connected. |

#### dberror:

| Source        | Code                       | Message | Description                    |
| ------------- | -------------------------- | ------- | ----------------------------------- |
| DBLIB_CONNECT | RECONNECT_ATTEMPT_EXCEEDED   |                                 | Triggered when `reconnect_failed` event is raised.           |
| DBNET_CONNECT | DISCONNECT_REQUEST           |                                 | Triggered when `connect_error` event is raised.              |
| DBNET_CONNECT | RECONNECT_REQUEST            |                                 | Triggered when `connect_error`, `connection_break`  event is raised. |
| DBLIB_CONNECT | NETWORK_DISCONNECTED         |                                 | Triggered when `connect_error`, `reconnect_error`  event is raised. |
| DBLIB_CONNECT | INVALID_URL                  |                                 | Value of `dbridge.auth_url` is not a valid dataBridges authentication URL. |
| DBLIB_CONNECT | INVALID_AUTH_PARAM           |                                 | Value of `dbridge.appkey` is not a valid dataBridges application key. |
| DBLIB_CONNECT | INVALID_ACCESSTOKEN_FUNCTION |                                 | If *"callback function"* is not declared for authentication  **only** while using  **private, presence or system** channel/RPC functions. *(Check Channel or RPC section for details.)* |
| DBLIB_CONNECT | HTTP_                        | HTTP protocol reported message. | HTTP Errors returned during authentication process. ***HTTP Error code*** will be concatenated with `HTTP_` in the `err.code`. `eg. HTTP_501` |
| DBLIB_CONNECT | INVALID_CLIENTFUNCTION       |                                 | If *"callback function"* is not declared for client function **or** `typeof()` variable defined is not a *"function"*. This is applicable only if clientFunction is enabled. *(Check Client Function section for details.)* |

#### Exceptions: 

| Source             | Code              | Description                                                  |
| ------------------ | ----------------- | ------------------------------------------------------------ |
| DBLIB_CONNECT_BIND | INVALID_EVENTNAME | Invalid Event name. Not in defined events as above.          |
| DBLIB_CONNECT_BIND | INVALID_CALLBACK  | If *"callback function"* is not declared **or** `typeof()` variable defined is not a *"function"*. |

### Functions

#### rttping()

This method is to understand the latency in milliseconds between your application and the dataBridges router where your application is connected. Event `rttpong` is triggered once response  is received from dataBridges network. Bind to event:`rttpong` to retrieve the latency in ms. 

```c#
// To get the last known Latency in milliseconds between your application and the dataBridges router where your application is connected.
// The dataBridges library exchanges rttms during the initial dataBridges network connection routine.
Console.WriteLine(dbridge.connectionstate.rttms);

// To get the latest Latency in milliseconds between your application and the dataBridges router where your application is connected.
try {
    dbridge.connectionstate.rttping();
} catch (dBError err){
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

// Bind to rttpong, to get notified about the latest Latency in milliseconds between your application and the dataBridges router where your application is connected.

void rttpong(object payload) {
    Console.WriteLine(payload);
    //payload is an integer value is in millisecond which is same as dbridge.connectionstate.rttms.
}

try {
    Action<object> rttpong_callback = rttpong;
    dbridge.connectionstate.bind("rttpong",rttpong_callback);
} catch (dBError err){
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

#### Exceptions: 

| Source        | Code                 | Description                                      |
| ------------- | -------------------- | ------------------------------------------------ |
| DBLIB_RTTPING | NETWORK_DISCONNECTED | Connection to dataBridges network is not active. |



------



## object:Channel

channel object exposes **trust-safe** flexible Pub/Sub messaging properties, functions and events to build realtime event messaging / event driven applications at scale.

Concepts

- A message is attached to an event
- Group similar events into a channel
- Subscribe to a channel to receive all channel event messages. 
- Publish event message to the channel and it will be sent to all the channel subscribers who are connected to dataBridges network and online.
- if you need to have an access controlled channel, prefix the channel name with pvt: , prs: and sys: .To subscribe to these type of channel. you will need to pass a trust-token when you subscribe to the channel. A trust-token is a JWT document created using a combination of channelname + sessionid + app.secret. 
  - Use your existing access control,  authorization and session identification rule-set, process and methods to create a trust-token instructing the dataBridges router to accept the pvt: prs: and sys: channel subscription, connection of from client application.
- Trust-tokens allows you to enable secured, access controlled and compliance driven realtime event driven messaging in your existing and new initiative applications.

dataBridges library supports **4** types of channel. The *namespace is the  4 characters* preceding the channelName (`pvt:,prs:,sys:`), identifying which type of channel the application is connecting to. If the channel type is `pvt:,prs:,sys:`, dataBridges library will use the `access_token` function to get the access encrypted token and will use it for all communication with this channel.

| Channel Type | Channel Name Style | Description                                                  |
| ------------ | ------------------ | ------------------------------------------------------------ |
| Public       | channelName        | Public channel is used to send and receive messages that are to be publicly available. This channel type does not require any trust authorization token to subscribe. <br />*e.g  channelName =* `mychannel` |
| Private      | **pvt:**channeName | Private channels is restricted channel. application will need to provide trust authorization token to subscribe and use Private channel. The dataBridges library will use the `access_token` function to get the trust authorization token. <br />*e.g  channelName =* `pvt:mychannel` |
| Presence     | **prs:**channeName | Presence channels is a specialized private channel with additional feature of presence awareness. Subscribing to presence channel allows application to be notified of members joining / leaving the channel. Since Presence channel is a specialized version of Private channel, application will need to provide trust authorization token to subscribe and use Private channels. The dataBridges library will use the `access_token` function to get the trust authorization token.<br />*e.g  channelName =* `prs:mychannel` |
| System       | **sys:**channeName | System channel is a specialized Presence channel to build command and control applications. Using System channel to create command and control server applications to send messages to devices and application using dataBridges library in **trust-safe manner **, build smart update configuration system and implement **trust-safe ** actions for remote and automated management. System channel allows application to send and receive messages with the server application (using dataBridges server library).  Since System channel is a specialized version of Presence channel, application will need to provide trust authorization token to subscribe and use Private channels. The dataBridges library will use the `access_token` function to get the trust authorization token.<br />*e.g  channelName =* `sys:systeminfo` |

dataBridges library provides 2 different ways to access any of above channel types based on usage.

| Channel Type         | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| Subscribe to Channel | application that subscribes to a channel will receive messages and can send messages. |
| Connect to Channel   | Where we have use-cases where application needs to only send messages and not interested to consume / receive channel messages, should connect to channel instead of subscribing to the channel. |

### Subscribe to Channel

Application that subscribes to a channel will receive messages and can send messages.

#### subscribe()

The default method for subscribing to a channel involves invoking the `channel.subscribe` function of your dataBridges object:

```c#
try {
    dBridges.channel.channel subscribed_channel = dbridge.channel.subscribe('mychannel');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

| Parameter | Rules                                                        | Description                                     |
| --------- | ------------------------------------------------------------ | ----------------------------------------------- |
| `string`  | *channelName **OR**<br />**pvt:**channelName **OR**<br />**prs:**channelName **OR**<br />**sys:**channelName* | *channelName* to which subscription to be done. |

| Return Type | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| `object`    | *channel* object which events and related functions can be bound to. |

Application can directly work with dataBridges object without using Channel object. Using this method application **cannot publish** any events.

```c#
try {
    dbridge.channel.subscribe('mychannel');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

##### Exceptions: 

| Source                  | Code                       | Description                                                  |
| ----------------------- | -------------------------- | ------------------------------------------------------------ |
| DBLIB_CHANNEL_SUBSCRIBE | NETWORK_DISCONNECTED       | Connection to dataBridges network is not active.             |
| DBLIB_CHANNEL_SUBSCRIBE | INVALID_CHANNELNAME        | Applicable for below conditions <br />1. *channelName* is not defined.<br />2. *channelName* validation error, `typeof()`  *channelName*  is not type string<br />3. *channelName* validation error, *channelName* fails `a-zA-Z0-9\.:_-` validation. |
| DBLIB_CHANNEL_SUBSCRIBE | INVALID_CHANNELNAME_LENGTH | *channelName* validation error, length of *channelName*  greater than **64** |
| DBLIB_CHANNEL_SUBSCRIBE | CHANNEL_ALREADY_SUBSCRIBED | *channelName* is already subscribed.                         |

#### unsubscribe() 

To unsubscribe from a subscribed channel, invoke the `unsubscribe` function of your dataBridges object. `unsubscribe` cannot be done on channel object.

```c#
try {
    dbridge.channel.unsubscribe('mychannel');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

| Parameter | Rules                                                        | Description                                        |
| --------- | ------------------------------------------------------------ | -------------------------------------------------- |
| `string`  | *channelName **OR**<br />**pvt:**channelName **OR**<br />**prs:**channelName **OR**<br />**sys:**channelName* | *channel*Name to which un-subscription to be done. |

| Return Type | Description |
| ----------- | ----------- |
| `NA`        |             |

##### Exceptions: 

| Source                    | Code                          | Description                                                  |
| ------------------------- | ----------------------------- | ------------------------------------------------------------ |
| DBLIB_CHANNEL_UNSUBSCRIBE | NETWORK_DISCONNECTED          | Connection to dataBridges network is not active.             |
| DBLIB_CHANNEL_UNSUBSCRIBE | CHANNEL_NOT_SUBSCRIBED        | *channelName* is not subscribed.                             |
| DBLIB_CHANNEL_UNSUBSCRIBE | INVALID_CHANNEL_TYPE          | *channelName* is not subscribed, but it is in connected state. |
| DBLIB_CHANNEL_UNSUBSCRIBE | UNSUBSCRIBE_ALREADY_INITIATED | unsubscription to the channel is already initiated and hence the current unsubscribe command exited with exception. |

### Connect to Channel

Use-cases where application needs to only send channel messages and not interested to consume / receive channel messages, should connect to channel instead of subscribing to the channel.

#### connect()

The default method for connecting to a channel involves invoking the `channel.connect` function of your dataBridges object. Application cannot publish system events for which it has connected to. 

```c#
try {
    dBridges.channel.channelnbd connected_channel = dbridge.channel.connect('mychannel');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

| Parameter | Rules                                                        | Description                                   |
| --------- | ------------------------------------------------------------ | --------------------------------------------- |
| `string`  | *channelName **OR**<br />**pvt:**channelName **OR**<br />**prs:**channelName **OR**<br />**sys:**channelName* | *channel*Name to which connection to be done. |

| Return Type | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| `object`    | *channel* object which events and related functions can be bound to. |

##### Exceptions: 

| Source                | Code                      | Description                                                  |
| --------------------- | ------------------------- | ------------------------------------------------------------ |
| DBLIB_CHANNEL_CONNECT | NETWORK_DISCONNECTED      | Connection to dataBridges network is not active.             |
| DBLIB_CHANNEL_CONNECT | CHANNEL_ALREADY_CONNECTED | *channelName* is already connected.                          |
| DBLIB_CHANNEL_CONNECT | INVALID_CHANNELNAME       | Applicable for below conditions<br />1. *channelName* is not defined.<br />2. *channelName* validation error, `typeof()`  *channelName*  is not type string<br />3. *channelName* validation error, length of *channelName*  greater than **64**<br />4. *channelName* validation error, *channelName* fails `a-zA-Z0-9\.:_-` validation.<br />5. if *channelName* contains `:` and first token is not `pvt,prs,sys` |

#### disconnect() 

To disconnect from a connected channel, invoke the `disconnect` function of your dataBridges object. `disconnect` cannot be done on channel object.

```c#
try {
    dbridge.channel.disconnect('mychannel');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```
| Parameter | Rules                                                        | Description                                      |
| --------- | ------------------------------------------------------------ | ------------------------------------------------ |
| `string`  | *channelName **OR**<br />**pvt:**channelName **OR**<br />**prs:**channelName **OR**<br />**sys:**channelName* | *channel*Name to which disconnection to be done. |

| Return Type | Description |
| ----------- | ----------- |
| `NA`        |             |

##### Exceptions: 

| Source                   | Code                         | Description                                                  |
| ------------------------ | ---------------------------- | ------------------------------------------------------------ |
| DBLIB_CHANNEL_DISCONNECT | NETWORK_DISCONNECTED         | Connection to dataBridges network is not active.             |
| DBLIB_CHANNEL_DISCONNECT | DISCONNECT_ALREADY_INITIATED | disconnect to the channel is already initiated and hence the current disconnect command exited with exception. |
| DBLIB_CHANNEL_DISCONNECT | INVALID_CHANNEL              | *channelName* is not connected.                              |
| DBLIB_CHANNEL_DISCONNECT | INVALID_CHANNEL_TYPE         | *channelName* is not connected, but it is in subscribed state. |

### Channel Information

#### isOnline()

*<u>dBridgeObject</u>* as well as *<u>channelObject</u>* provides a function to check if the channel is online. The best practice is to check the channel is online before publishing any message.

```c#
bool isonline = dbridge.channel.isOnline('mychannel');
```

```c#
bool isonline = subscribed_channel.isOnline(); 
```

| Parameter | Rules                                                        | Description   |
| --------- | ------------------------------------------------------------ | ------------- |
| `string`  | *channelName **OR**<br />**pvt:**channelName **OR**<br />**prs:**channelName **OR**<br />**sys:**channelName* | *channel*Name |

| Return Values | Description                                         |
| ------------- | --------------------------------------------------- |
| `boolean`     | Is the current status of channel online or offline. |

#### list()

<u>*dBridgeObject*</u>  provides a function to get list of successfully subscribed or connected channel. 

```c#
List<Dictionary<string, object>> channels = dbridge.channel.list();
#=> [{"name":  "mychannel" , "type": "subscribed/connect" ,  "isonline": true/false }];
```

| Return Type     | Description                                |
| --------------- | ------------------------------------------ |
| `array of dict` | Array of channels subscribed or connected. |

Dictionary contains below information.

| Key        | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| `name`     | *(string)* *channelName* of subscribed or connected channel. |
| `type`     | *(string)* `subscribed` **or** `connected`                   |
| `isonline` | *(boolean)* Is the current status of channel online or offline. |

#### getChannelName() 

*<u>channelObject</u>* provides a function to get the *channelName*. 

```c#
string chName = channelobject.getChannelName() 
```

| Return Type | Description                                       |
| ----------- | ------------------------------------------------- |
| `string`    | *channelName* of subscribed or connected channel. |

### Publish to Channel

Publish event-message using the `publish` function on an instance of the `channel` object.

A message is linked to an event and hence event-message. dataBridges allows you to bind to various events to create rich event driven processing flows

#### publish()

The default method for publishing to a channel involves invoking the `channel.publish` function of your *channelObject*. 

```c#
try {
  channelObject.publish(event, payload, excludeSessionId, sourceId, seqno);
} catch (dBError err) {
  Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

// Best practice is to check the channel is online before publishing any message.
if (channelObject.isOnline()) {
  try {
    channelObject.publish(event, payload, excludeSessionId, sourceId, seqno);
  } catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
  }    
}
```

| Parameter | Description                                                  |
| --------- | ------------------------------------------------------------ |
| `event`   | *(string)* *event* Name to which the message to be sent. *event* Name cannot start with `dbridges:` |
| `payload` | *(string)* Payload to be sent with the event.                |
| `seqno`   | *(string) [optional]* Message sequence number. This is optional parameter. `seqno` can be used by applications to manage message queue processing by the subscribers. |

| Return Values | Description |
| ------------- | ----------- |
| `NA`          |             |

##### Exceptions: 

| Source                | Code                 | Description                                                  |
| --------------------- | -------------------- | ------------------------------------------------------------ |
| DBLIB_CHANNEL_PUBLISH | INVALID_SUBJECT      | Applicable for below conditions<br />1. *event* validation error, `typeof()`  *event*  is not type string<br />2.  *event* validation error,  *event*  is not defined |
| DBLIB_CHANNEL_PUBLISH | NETWORK_DISCONNECTED | Connection to dataBridges network is not active.             |

### Binding to events

A message is linked to an event and hence event-message. dataBridges allows you to bind to various events to create rich event processing flows. An application needs to bind to event to process the received message. 

You can use the following methods either on a *channelObject*, to bind to events on a particular channel; or on the *dbridgeObject*, to bind to events on all subscribed channels simultaneously.

#### `bind` and `unbind`
**Bind** to "event" on channel: payload and metadata is received.

```c#
//  Binding to channel events on channelObject using Anonymous function.
Action<object,object> iChannelStatus;
try {
    channelObject.bind('eventName',  iChannelStatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
// Binding to channel events on dbridgeObject 

try {
    dbridge.channel.bind('eventName', iChannelStatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

// Binding to channel events on channelObject using user function.
void channelStatus(object payload, object metadata){
    Console.WriteLine("{0} , {1} " , payload, metadata);
}

try {
    Action<object,object> iChannelStatus = channelStatus;
    dbridge.channel.bind('eventName', iChannelStatus);
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

| Parameter | Description                                          |
| --------- | ---------------------------------------------------- |
| `event`   | *(string)* *event* Name to which binding to be done. |

##### Callback parameters

###### payload: 

`(string)` Payload data sent by the publisher.

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "event",				// (string) eventName 
    "sourcesysid": "", 					// (string) Sender system identity, applicable only for presence or system channel.
    "sqnum": "1",						// (string) user defined, sent during publish function.
    "sessionid": "", 					// (string) Sender sessionid, applicable only for presence or system channel.
    "intime": 1645554960732  			// (string) EPOC time of the sender at time of publish.
}
```

##### Exceptions: 

| Source                | Code                         | Description                                                  |
| --------------------- | ---------------------------- | ------------------------------------------------------------ |
| DBLIB_CONNECT_BIND    | INVALID_EVENTNAME            | *eventName* cannot be blank or null.                         |
| DBLIB_CONNECT_BIND    | INVALID_CALLBACK             | If *"callback function"* is not declared **or** `typeof()` variable defined is not a *"function"*. |
| DBLIB_CHANNEL_CONNECT | INVALID_CHANNEL_TYPE_BINDING | Invalid Event name. This binding is not allowed in `channel.connect`. |

**Unbind** behavior varies depending on which parameters you provide it with. For example:

```c#
// Remove just `handler` of the `event` in the subscribed/connected channel 
channelObject.unbind("eventName",handler);

// Remove all `handler` of the `event` in the subscribed/connected channel
channelObject.unbind("eventName");

//Remove all handlers for the all event in the subscribed/connected channel
channelObject.unbind();

// Remove `handler` of the `event` for all events across all subscribed/connected channels
dbridge.channel.unbind("eventName",handler);

// Remove all handlers of the `event` for all events across all subscribed/connected channels
dbridge.channel.unbind("eventName");

// Remove all handlers for all events across all subscribed/connected channels
dbridge.channel.unbind();
```

#### `bind_all` and `unbind_all`

`bind_all` and `unbind_all` work much like `bind` and `unbind`, but instead of only firing callbacks on a specific event, they fire callbacks on any event, and provide that event in the metadata  to the handler along with the payload. `bind_all` and `unbind_all` is not available for `connected_channel` i.e. `dbridge.channel.connect()` object.

```c#
//  Binding to all channel events on channelObject  
try {
    Action<object , object> ichannelstatus;
    channelObject.bind_all(ichannelstatus =  (payload, metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

// Binding to all channel events on dbridgeObject 

try {
    Action<object , object> ichannelstatus;
    dbridge.channel.bind_all(ichannelstatus =  (payload, metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

Callback out parameter `payload, metadata` details are explained with each event below in this document.

##### Exceptions: 

| Source             | Code             | Description                                                  |
| ------------------ | ---------------- | ------------------------------------------------------------ |
| DBLIB_CONNECT_BIND | INVALID_CALLBACK | If *"callback function"* is not declared **or** `typeof()` variable defined is not a *"function"*. |

`unbind_all` works similarly to `unbind`.

```c#
// Remove just `handler` across the channel 
channelObject.unbind_all(handler);

//Remove all handlers for the all event in the subscribed/connected channel
channelObject.unbind_all();

// Remove `handler` across the subscribed/connected channels
dbridge.channel.unbind_all(handler);

// Remove all handlers for all events across all subscribed/connected channels
dbridge.channel.unbind_all();
```

### System events for channel object

There are a number of events which are triggered internally by the library, but can also be of use elsewhere. Below are the list of all events triggered by the library.

Below syntax is same for all system events.

```c#
//  Binding to systemevent on channelObject  
Action<object,object> iChannelStatus;
try {
    channelObject.bind('dbridges:subscribe.success',  iChannelStatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

// Binding to systemevent on dbridgeObject 
try {
    dbridge.channel.bind('dbridges:subscribe.success', iChannelStatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

#### dbridges:subscribe.success 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 			// (string) channelName to which subscription is done.
    "eventname": "dbridges:subscribe.success",// (string) eventName 
    "sourcesysid": "", 					// (string) Sender system identity, applicable only for presence or system channel.
    "sqnum": "1",						// (string) user defined, sent during publish function.
    "sessionid": "", 					// (string) Sender sessionid, applicable only for presence or system channel.
    "intime": 1645554960732  			// (string) EPOC time of the sender at time of publish.
}
```

#### dbridges:subscribe.fail 

##### Callback parameters

###### payload:  `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:subscribe.fail",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:channel.online 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:channel.online",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:channel.offline   

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:channel.offline",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:channel.removed

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:channel.removed",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:unsubscribe.success

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:unsubscribe.success",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:unsubscribe.fail

##### Callback parameters

###### payload:  `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:unsubscribe.fail",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:resubscribe.success 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:resubscribe.success",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:resubscribe.fail

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:resubscribe.fail",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:participant.joined  

This will be triggered only for **presence** `(prs:)` and **system** `(sys:)` channel subscription.

##### Callback parameters

###### payload: `(dict)`

```c#
{
  "sessionid": "ydR27s3Z92yQw7wjGY2lX", 	// (string) Session id of the member who has subscribed/connected to channel
  "libtype": "nodejs", 						// (string) Library Lang of the member who has subscribed/connected to channel
  "sourceipv4": "0.0.0.0", 					// (string) IPv4 of the member who has subscribed/connected to channel
  "sourceipv6": "::1", 						// (string) Not Applicable in current version.
  "sysinfo": '{"sysid":"nameofcaller"}' 	// (string) System Info of the member who has subscribed/connected to channel
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "prs:channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:participant.joined",// (string) eventName 
    "sourcesysid": "nameofcaller", 			// (string) Sys id of the member who has subscribed/connected to channel
    "sqnum": null,							// (string) 
    "sessionid": "ydR27s3Z92yQw7wjGY2lX", 	// (string) Session id of the member who has subscribed/connected to channel
    "intime": null	  						// (string) 
}
```

#### dbridges:participant.left 

This will be triggered only for **presence** `(prs:)` and **system** `(sys:)` channel subscription.

##### Callback parameters

###### payload: `(dict)`

```c#
{
  "sessionid": "ydR27s3Z92yQw7wjGY2lX", 	// (string) Session id of the member who has subscribed/connected to channel
  "libtype": "nodejs", 						// (string) Library Lang of the member who has subscribed/connected to channel
  "sourceipv4": "0.0.0.0", 					// (string) IPv4 of the member who has subscribed/connected to channel
  "sourceipv6": "::1", 						// (string) Not Applicable in current version.
  "sysinfo": '{"sysid":"nameofcaller"}' 	// (string) System Info of the member who has subscribed/connected to channel
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "prs:channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:participant.left",// (string) eventName 
    "sourcesysid": "nameofcaller", 			// (string) Sys id of the member who has subscribed/connected to channel
    "sqnum": null,							// (string) 
    "sessionid": "ydR27s3Z92yQw7wjGY2lX", 	// (string) Session id of the member who has subscribed/connected to channel
    "intime": null	  						// (string) 
}
```

#### dbridges:connect.success 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:connect.success",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:connect.fail  

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:connect.fail",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:disconnect.success 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:disconnect.success",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:disconnect.fail  

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:disconnect.fail",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:reconnect.success 

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:reconnect.success",// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### dbridges:reconnect.fail  

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "dberror.Source" , 		// (string) Error source, Refer dberror: for details
    "code": "dberror.Code",				// (string) Error code, Refer dberror: for details
    "message": "" 						// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "channelname": "channelName" , 		// (string) channelName to which subscription is done.
    "eventname": "dbridges:reconnect.fail",				// (string) eventName 
    "sourcesysid": "", 					// (string) 
    "sqnum": "",						// (string) 
    "sessionid": "", 					// (string) 
    "intime": 	  						// (string) 
}
```

#### System events - payload (dberror object) - details:

| Source                    | Code              | Description                                                  |
| ------------------------- | ----------------- | ------------------------------------------------------------ |
| DBNET_CHANNEL_SUBSCRIBE   | ERR_FAIL_ERROR    | dataBridges network encountered error when subscribing to the channel. |
| DBNET_CHANNEL_SUBSCRIBE   | ERR_ACCESS_DENIED | dBrdige network reported **access violation** with `access_token` function during subscription of this channel. <br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to publish messages to public channels` is selected. |
| DBLIB_CHANNEL_SUBSCRIBE   | ACCESS_TOKEN      | `dbridge.access_token` function returned error.              |
| DBNET_CHANNEL_UNSUBSCRIBE | ERR_FAIL_ERROR    | dataBridges network encountered error when unsubscribing to the channel. |
| DBNET_CHANNEL_UNSUBSCRIBE | ERR_ACCESS_DENIED | dataBridges network reported **access violation** with `access_token` function during unsubscribing of this channel. <br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to publish messages to public channels` is selected. |
| DBNET_CHANNEL_CONNECT     | ERR_FAIL_ERROR    | dataBridges network encountered error when subscribing to the channel. |
| DBNET_CHANNEL_CONNECT     | ERR_ACCESS_DENIED | dataBridges network reported **access violation** with `access_token` function during subscription of this channel. <br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to publish messages to public channels` is selected. |
| DBLIB_CHANNEL_CONNECT     | ACCESS_TOKEN      | `dbridge.access_token` function returned error.              |
| DBLIB_CHANNEL_DISCONNECT  | ERR_FAIL_ERROR    | dataBridges network encountered error when unsubscribing to the channel.. |
| DBLIB_CHANNEL_DISCONNECT  | ERR_ACCESS_DENIED | dataBridges network reported **access violation** with `access_token` function during unsubscribing of this channel. <br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to publish messages to public channels` is selected. |



------



## object: rpc (Remote Procedure Call)

rpc object exposes **trust-safe** properties, functions and events to provide reliable two-way messaging (request-response) between multiple endpoints allowing you to build sophisticated asynchronous interactions.

Concepts

- Client application  allows you to execute RPC functions exposed by server applications. 
- Client application is called CALLEE and the server application is called CALLER.
- Client application will execute a remote function by passing IN.paramter, and a timeout
  - The server application's corresponding function will be invoked with the IN.parameter and it will respond with response() or exception() which will be delivered back to the CALLEE client application by dataBridges network completing the request-response communication.
-  Client application need not be aware about RPC servers identity and will only interact with RPC server namespace. The dataBridges network will intelligently route and load balance RPC call() to the RPC server application. The dataBridges network will automatically load balance multiple instance of server application exposing the same RPC endpoints.
- Trust-tokens are supported by RPC as well. You will need to pass a trust-token when you connect to the access controlled RPC endpoint. A trust-token is a JWT document created using a combination of rpc server endpoint / server name + sessionid + app.secret. 
  - Use your existing access control,  authorization and session identification rule-set, process and methods to create a trust-token instructing the dataBridges router to accept the pvt: prs: rpc endpoint/server connection of from client application.
- Trust-tokens allows you to enable secured, access controlled and compliance driven reliable two-way messaging (request-response)  in your existing and new initiative applications.

### Connect to Server

To use rpc functions, the application has to connect to the rpc endpoint/server. This is done using `connect()` function explained below.

#### connect()

The default method for connecting to a rpc endpoint/server involves invoking the `rpc.connect` function of your dataBridges object.

```c#
using  dBridges.remoteprocedure;
try {
    CrpCaller rpcClient = dbridge.rpc.connect('rpcServer');
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}   
```

| Parameter | Rules                                                        | Description                                  |
| --------- | ------------------------------------------------------------ | -------------------------------------------- |
| `string`  | *serverName  **OR**<br />**pvt:**serverName **OR**<br />**prs:**serverName* | *server*Name to which connection to be done. |

| Return Type | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| `object`    | *rpcObject* which events and related functions can be bound to. |

##### Exceptions: 

| Source            | Code                 | Message | Description                                                  |
| ----------------- | -------------------- | ------- | ------------------------------------------------------------ |
| DBLIB_RPC_CONNECT | INVALID_SERVERNAME   |         | Applicable for below conditions <br />1. *serverName* is not defined.<br />2. *serverName* validation error, length of *serverName* greater than **64**<br />3. *serverName* validation error, *serverName* fails `a-zA-Z0-9\.:_-` validation.<br />4. *serverName* contains `:` and first token is not `pvt,prs`. |
| DBLIB_RPC_CONNECT | NETWORK_DISCONNECTED |         | Connection to dataBridges network is not active.             |
| DBNET_RPC_CONNECT | ERR_FAIL_ERROR       |         | dataBridges network encountered error during current operation. |
| DBNET_RPC_CONNECT | ERR_ACCESS_DENIED    |         | dataBridges network reported **access violation** with `access_token` function during current operation.<br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to access RPC functions` is selected. |

### Server Information

#### isOnline()

*<u>rpcObject</u>* provides a function to check if the channel is online. 

```c#
bool isonline = rpcClient.isOnline(); 
```

| Parameter | Rules                                                        | Description                                    |
| --------- | ------------------------------------------------------------ | ---------------------------------------------- |
| `string`  | *serverName  **OR**<br />**pvt:**serverName **OR**<br />**prs:**serverName* | *server*Name to which subscription to be done. |

| Return Values | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| `boolean`     | Is the current status of server connection online or offline. |

#### getServerName() 

*<u>rpcObject</u>* provides a function to get the *serverName*. 

```c#
string serverName = rpcClient.getServerName() 
```

| Return Type | Description                          |
| ----------- | ------------------------------------ |
| `string`    | *serverName* of connected rpcServer. |

### Execute Remote Procedure Call

#### call() 

*<u>rpcObject</u>*  call() function allows you to execute a remote function hosted by RPC endpoint / server using dataBridges server library

- passing function parameter as parameter
- while setting an time to live (TTL) for the response 

The RPC call() functions supports multipart response (where the RPC function can send back multiple responses to a single RPC function call) along with exception routine.

```c#
using RSG;
using dBridges.remoteprocedure;

Action<object> iprogress;
IPromise<object> p = rpcClient.call(functionName, parameter, ttlms, iprogress = (response) => {
    Console.WriteLine("multipart response {0}",response)
});
p.Then((response) => {
    Console.WriteLine("end response {0}", response)
}).Catch((err) => {
    Console.WriteLine("exception {0}", err);
});

//Below example how a application can connect to a RPC endpoint / Server called mathServer and use add, multiply functions.
try {
    CrpCaller myMathServer = dbridge.rpc.connect('mathServer');
}catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}

Dictionary<string , int> obj = new Dictionary<string,int32>() { {"num1",44.5} , {"num2",30} };
inparam = new JavaScriptSerializer().Serialize(obj);

Action<object> iprogress;
IPromise<object> p = myMathServer.call("add", inparam, ttlms, iprogress = (response) => {
    Console.WriteLine("multipart response {0}",response)
});
p.Then((response) => {
    Console.WriteLine("end response {0}", response)
}).Catch((err) => {
    Console.WriteLine("exception {0}", err);
});

Action<object> iprogress;
IPromise<object> p = myMathServer.call("multiply", inparam, ttlms, iprogress = (response) => {
    Console.WriteLine("multipart response {0}",response)
});
p.Then((response) => {
    Console.WriteLine("end response {0}", response)
}).Catch((err) => {
    Console.WriteLine("exception {0}", err);
});
```

| Parameter      | Expected Value       | Description                                                  |
| -------------- | -------------------- | ------------------------------------------------------------ |
| `functionName` | *functionname*       | *(string)* Function name as defined in *rpc endpoint/ Server* . <br />Note - RPC endpoint / server can expose multiple rpc functions. |
| parameter      | *function parameter* | *(string)* if multiple parameters to be passed, This can be done by putting it into array or json and stringify the object. |
| ttlms          | `1000`               | *(integer)* Time to live in millisecond, timeout value before the call() function throws error timeout. |

| Return Values | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| `string`      | Multipart or final response. in case of error, dberror object is returned. |

##### Exceptions: 

| Source               | Code                      | Description                                                  |
| -------------------- | ------------------------- | ------------------------------------------------------------ |
| DBNET_RPC_CALL       | NETWORK_DISCONNECTED      | Connection to dataBridges network is not active.              |
| DBNET_RPC_CALL       | RESPONSE_TIMEOUT          | call() response not received within defined `ttlms`. |
| DBLIB_RPC_CALL       | ID_GENERATION_FAILED      | Internal Library error.                                      |
| DBNET_RPC_CALL       | ERR_ACCESS_DENIED         | dataBridges network reported **access violation** with `access_token` function during current operation.<br />Verify if appKey has sufficient publish grants. Login to management portal. Select `Edit Key` option and check `Allow key to access RPC functions` is selected. |
| DBRPCCALLEE_RPC_CALL | ERR_`error_code`          | This indicates an exception encountered by the remote RPC function. ERR_error_code will have the details. |
| DBNET_RPC_CALL       | CLE_NR_10865         | rpc endpoint / server disconnected from dataBridges network. Try again. |
| DBNET_RPC_CALL       | CLE_NR_30391         | rpc endpoint / server disconnected from dataBridges network. Try again. |
| DBNET_RPC_CALL       | CLE_QX_41074         | Cannot process the call() because the RPC server (in this case CALLEE) has exceeded outstanding pending rpc call() queue limit. |
| DBNET_RPC_CALL       | CLE_QX_49467         | Cannot process the call() because the RPC server (in this case CALLEE) has exceeded outstanding pending rpc call() queue limit. |
| DBNET_RPC_CALL       | CLR_QX_39305         | Cannot process the call() because the application (in this case CALLER) has exceeded outstanding pending rpc call() queue limit. |
| DBNET_RPC_CALL       | CLR_QX_39824         | Cannot process the call() because the application (in this case CALLER) has exceeded outstanding pending rpc call() queue limit. |
| DBNET_RPC_CALL       | RE_28710             | rpc endpoint / server disconnected from dataBridges network. Try again. |
| DBNET_RPC_CALL       | AD_48621             | Application does not have access to execute rpc functions. |

### System events for rpc object

There are a number of events which are triggered internally by the library, but can also be of use elsewhere. Below are the list of all events triggered by the library.

Below syntax is same for all system events.

```c#
//  Binding to systemevent on rpcObject  
try {
    Action<object ,object> ieventstatus;
    rpcClient.bind('eventName',  ieventstatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1}", payload, metadata);
    });
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}   
// Binding to systemevent on dbridgeObject 

try {
    Action<object ,object> ieventstatus;
    dbridge.rpc.bind('eventName', ieventstatus = (object payload, object metadata) => {
        Console.WriteLine("{0} , {1}", payload, metadata);
	});
} catch (dBError err) {
 	Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}   
```

#### `bind_all` and `unbind_all`

`bind_all` and `unbind_all` work much like `bind` and `unbind`, but instead of only firing callbacks on a specific event, they fire callbacks on any event, and provide that event in the metadata  to the handler along with the payload. 

```c#
//  Binding to all rpc events on rpcObject  
try {
    Action<object , object> ichannelstatus;
    rpcClient.bind_all(ichannelstatus =  (payload, metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
// Binding to all rpc events on dbridgeObject 

try {
    Action<object , object> ichannelstatus;
    dbridge.rpc.bind_all(ichannelstatus =  (payload, metadata) => {
        Console.WriteLine("{0} , {1} " , payload, metadata);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

Callback out parameter `payload, metadata` details are explained with each event below in this document.

##### Exceptions: 

| Source             | Code             | Description                                                  |
| ------------------ | ---------------- | ------------------------------------------------------------ |
| DBLIB_CONNECT_BIND | INVALID_CALLBACK | If *"callback function"* is not declared **or** `typeof()` variable defined is not a *"function"*. |

`unbind_all` works similarly to `unbind`.

```c#
// Remove just `handler` across the connected rpc server
rpcClient.unbind_all(handler);

//Remove all handlers for the all event in the connected rpc server
rpcClient.unbind_all();

// Remove `handler` across the connected rpc servers
dbridge.rpc.unbind_all(handler);

// Remove all handlers for all events across all connected rpc servers
dbridge.rpc.unbind_all();
```

#### dridges:rpc.server.connect.success

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "servername": "serverName" , 					// (string) serverName to which connection is done.
    "eventname": "dbridges:rpc.server.connect.success", // (string) eventName 
}
```

#### dbridges:rpc.server.connect.fail

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "DBLIB_RPC_CONNECT" , 	// (string) Error source
    "code": "ACCESS_TOKEN_FAIL",			// (string) Error code 
    "message": "" 							// (string) Error message if applicable.
}
```

###### metadata `(dict)`:

```c#
{
    "servername": "serverName" , 				// (string) serverName to which connection is done.
    "eventname": "dbridges:rpc.server.connect.fail",// (string) eventName 
}
```

#### dbridges:rpc.server.online

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "servername": "serverName" ,           // (string) serverName to which connection is done.
    "eventname": "dbridges:rpc.server.online", // (string) eventName 
}
```

#### dbridges:rpc.server.offline

##### Callback parameters

###### payload: 

`null` 

###### metadata `(dict)`:

```c#
{
    "servername": "serverName" ,                    // (string) serverName to which connection is done.
    "eventname": "dbridges:rpc.server.offline",// (string) eventName 
}
```

#### dberror:  

| Source            | Code              | Message         | Description                                                  |
| ----------------- | ----------------- | --------------- | ------------------------------------------------------------ |
| DBLIB_RPC_CONNECT | ACCESS_TOKEN_FAIL |                 | Specific to **private** `(pvt:)` or **presence** (`prs:`) rpc call. Access token validation failed at dataBridges network. |
| DBLIB_RPC_CONNECT | ACCESS_DENIED     | `error_message` | Specific to **private** `(pvt:)` or **presence** (`prs:`) rpc call. This is returned by the `access_token` function execution before `call()` is made. |



------



## object:Cf (Client Function)

CF (Client-function) object is a special purpose RPC | request-response implementation to build command and control applications. CF object exposes properties, functions and events for command and control server applications to send messages to devices and application using dataBridges library in **trust-safe manner **, build smart update configuration system and implement **trust-safe ** actions for remote and automated management.

CF REDUCES HUGE ENGINEERING TIME EFFORT REQUIRED TO DESIGN, BUILD AND MAINTAIN A ROBUST COMMAND-CONTROL INFRASTRUCTURE.

- A client function(s)  is a callback function exposed by the client library as a RPC (remote procedure call). 
- Server application (using dataBridges server library), can execute the CF function remotely.

Concepts

- CF (client-function) simplifies the comand and control type application design and maintenance. 
- iOT and large distributed system requires a standard, secured and compliant method to send reliable  request-response communication to the managed devices from authenticated and authorized Command-and-Control server applications. dataBridges CF allows you you to expose device functions and capabilities in a easy, secured manner allowing only authoirized dataBridges server applications to communicate with the devices, remote applications.
- Only server application using dataBridges server library + application key secret can execute CF functions exposed by remote devices. 
  - The server application is called CALLER (the one executing cf.call() function)
  - The server application needs to know the sessionID of the device to which it needs to communicate.
  - The device application exposing command functions is called CALLEE. Only authenticated and authorized server application will be allowed to communicate with the device application for device / application management.

### Properties

The following is the list of *cf* properties. These properties has to be set before `dbridge.connect()`

| Property    | Description                                                  |
| ----------- | ------------------------------------------------------------ |
| `enable`    | *(boolean)* `(default:false)` If application wants to enable *clientFunction* functionality, this needs to be `true` else `false`. |
| `functions` | *(function)* A client function(s)  is a callback function exposed by the client library as a RPC (remote procedure call). |

#### enable:

You need to enable cf in the connection property.

```c#
dbridge.cf.enable = true;
```

#### functions:

Application can expose callback function(s) as Client function (special case RPC | Request-Response). Server application using dataBridges server library can remotely execute the client functions. Each function needs to be registered with the library as a client function (CF), using `dbridge.cf.regfn()`where you can link the functionName to ClientFunctionName.

- The client application that exposes the client function is called a CALLEE.
- The server application that executes the client function is called a CALLER.

Functions can be defined either inside the property callback function or anywhere in the scope of application. Below code exhibits both ways of exposing the function.

```c#
// function is exposed outside the property callback function, but in the scope of application.
using dBridges.responseHandler;

async void cfFunOutside(object inparam, object response)
{
    try {
        CResponseHandler responsehandler = response as CResponseHandler;
        responsehandler.tracker = true;
        Dictionary<string , string> uName = new Dictionary<string , string>() {"uname", "Linux analysis 2.6.32-696.30.1.el6.x86_64 #1 SMP Tue May 22 03:28:18 UTC 2018 x86_64 x86_64 x86_64 GNU/Linux"};

        responsehandler.next('retrieving uName');
        responsehandler.end(new JavaScriptSerializer().Serialize(uName));
        responsehandler.exception('INVALID_PARAM', 'Wrong parameter'); 
    } catch (dBError err) {
        Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
    }
}

async void cfFunctionBinder(object sender)
{
    try{
        dBridges.clientFunctions.cfclient cfclient = sender as dBridges.clientFunctions.cfclient;
        // function is exposed outside the property callback function, but in the scope of application.
        Action<object , object> ifunctionoutside = cfFunOutside;
        cfclient.regfn("nasa", ifunctionoutside);

        // function is exposed inside the property callback function.
        Action<object , object> ifunctioninside;
        cfclient.regfn("isro", ifunctioninside = (object inparam, object response) =>{
            try {
                CResponseHandler responsehandler = response as CResponseHandler;
                responsehandler.tracker = true;
                Dictionary<string , string> uName = new Dictionary<string , string>() {"uname", "Linux analysis 2.6.32-696.30.1.el6.x86_64 #1 SMP Tue May 22 03:28:18 UTC 2018 x86_64 x86_64 x86_64 GNU/Linux"};

                responsehandler.next('retrieving uName');
                responsehandler.end(new JavaScriptSerializer().Serialize(uName));
                responsehandler.exception('INVALID_PARAM', 'Wrong parameter'); 
            } catch (dBError err) {
                Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
            }       
        });

    } catch (dBError err) {
        Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
    }

}

Action<object> icfFunctions = cfFunctionBinder;
dbridge.cf.functions = icfFunctions;
// unbinding of function exposed by clientfunctions
dbridge.cf.unregfn("nasa", cfFunOutside);
```

Below are <u>*parameters*</u> of the callback function which is exposed to *clientfunctions*.

| Parameter  | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| `payload`  | *(string)*  The inParameter for the clientFunction.          |
| `response` | *(object)* The library creates a response object unique for each client function call. The Response object has *properties* and *function* to return execution results of the function back to caller. |

##### response: `(object)`

| Properties/Function | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| `tracker`           | *(boolean)* This will enable  response tracker, and event `cf.response.tracker` will be fired if any issue happens in sending back response to caller. Enable this property if your function needs a confirmation of reponse delivered to the caller. |
| `id`                | *(string)* *(readonly)* Each client function execution is assigned a unique ID by the library.  when the response tracker is enabled, the application can bind to an event `cf.response.tracker` to get the delivery notification. The event will indicate the delivery notification linked to this ID. Client application will need to maintain this ID to track the delivery notification. |
| `next`              | *(function)*  dataBridges CF (Special case RPC \|request-response) supports mult-part response. Application can use `response.next` to send multi-part response to the caller. |
| `end`               | *(function)*   `response.end` is to send the final response to the caller. Once `end` is called, the object is **closed** and no more response can be sent. |
| `exception`         | *(function)*  Two parameter, return `errorCode` *(string)* ,`errorMessage` *(string)* is sent to caller. This will raise an exception at the caller library. |

##### Exceptions:

Below exceptions are raised in the `cf.regfn`.

| Source         | Code                  | Description                                   |
| -------------- | --------------------- | --------------------------------------------- |
| DBLIB_CF_REGFN | INVALID_FUNCTION_NAME | Invalid Function name.                        |
| DBLIB_CF_REGFN | INVALID_CALLBACK      | Callback is not a function or is not defined. |

Below exceptions are raised on `response` object inside the registered function.

| Source        | Code                   | Description                                                  |
| ------------- | ---------------------- | ------------------------------------------------------------ |
| DBLIB_CF_CALL | NETWORK_DISCONNECTED   | Connection to dataBridges network is not active.             |
| DBLIB_CF_CALL | RESPONSE_OBJECT_CLOSED | Return response object is closed. Thus the function is unable to respond back to the call. |

#### resetqueue() 

*<u>dbridgeObject</u>*  resetqueue() . The dataBridges network maintains in-process CF function execution status. resetqueue() informs the dataBridges network that all in-process CF function execution will be dropped by the application and response to be invalidated. Resetqueue() use case is intended to be used by application in its self health status management. Sometime due to the application process flow, the application can identify situation where it would like to ease its load by resettiing the CF function execution queue by sending resetqueue() message to dataBridges network and than closing all in-process CF function execution. 

```c#
try {
    dbridge.cf.resetqueue();
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}
```

##### Exceptions: 

| Source        | Code                 | Description                                      |
| ------------- | -------------------- | ------------------------------------------------ |
| DBLIB_CF_CALL | NETWORK_DISCONNECTED | Connection to dataBridges network is not active. |

### System events for cf object

There are a number of events which are triggered internally by the library, but can also be of use elsewhere. Below are the list of all events triggered by the library.

Below syntax is same for all system events.

```c#
// Binding to systemevent on dbridgeObject 
try {
    Action<object , object> ifunction;
    dbridge.cf.bind('eventName', ifunction = (object payload, object response) =>{
        Console.WriteLine("{0} , {1} " , payload, response);
    });
} catch (dBError err) {
    Console.WriteLine("{0} ,  {1} , {2}" , err.source, err.code, err.message);
}   
```

#### cf.response.tracker

##### Callback parameters

| Return Values | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| `payload`     | *(string)*  Tracker identifier. which is same as `response.id` |
| `metadata`    | *(string)*  Refer below table.                               |

| Error Identifier | Description                                                  |
| ---------------- | ------------------------------------------------------------ |
| RE_12616         | cf caller is disconnected from dataBridges network and hence cannot process response tracking. |
| RE_13151         | cf caller is disconnected from dataBridges network and hence cannot process response tracking. |
| RE_30030         | The cf client is disconnected from dataBridges network       |
| RE_33635         | The cf client is disconnected from dataBridges network       |

#### cf.callee.queue.exceeded

##### Callback parameters

###### payload: `(dberror object)`

```c#
{
    "source": "DBNET_CF_CALL" , 				// (string) Error source
    "code": "ERR_CALLEE_QUEUE_EXCEEDED",		// (string) Error code  
    "message": "" 								// (string) Error message if applicable.
}
```

###### metadata:

`null`

#### dberror:

| Source        | Code                      | Description                                                  |
| ------------- | ------------------------- | ------------------------------------------------------------ |
| DBNET_CF_CALL | ERR_CALLEE_QUEUE_EXCEEDED | No new cf calls are being routed by the dataBridges network to the application because the application's current cf processing queue has already exceeded. <br />Each application connection cannot exceed cf.queue.maximum. Refer to management console documentation for cf.queue.maximum details. |



## Change Log
  * [Change log](CHANGELOG.md): Changes in the recent versions

## License

DataBridges Library is released under the [Apache 2.0 license](LICENSE).

```
Copyright 2022 Optomate Technologies Private Limited.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
