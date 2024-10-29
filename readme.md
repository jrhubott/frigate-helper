# Frigate-Helper

## Overview
Frigate-Helper is a .NET 8.0-based service designed to interact with MQTT brokers, particularly subscribing to and publishing messages related to object events. The primary purpose is to integrate with home automation systems like Home Assistant by processing and publishing MQTT data about real-time object detection from Frigate, a camera-based object detection system.

This service subscribes to Frigate’s MQTT topics, processes incoming events, tracks statistics, and then publishes those statistics and event metadata back to the MQTT broker.

## Features
- **MQTT Integration**: Connects to an MQTT broker to subscribe and publish events.
- **Event Handling**: Processes Frigate events like newly detected objects or updates to existing objects.
- **Statistics System**: Tracks and updates statistics about objects, such as how many are moving or stationary.
- **Home Assistant Integration**: Uses MQTT to publish processed information that can be easily integrated into automation systems like Home Assistant.

---

## Quick Start (Primary Method)

We recommend using Docker Compose, as it provides an easy setup without requiring manual dependencies. You can quickly download the `docker-compose.yml` and bring up the service.

### 1. Download the Docker Compose File

```bash
curl -O https://raw.githubusercontent.com/jrhubott/frigate-helper/main/docker-compose.yml
```

### 2. Update Docker Compose Configuration (Optional but Recommended)

Modify your `docker-compose.yml` file to point to your MQTT broker and customize other settings like topics and credentials:

```yaml
services:
  frigate-helper:
    image: ghcr.io/jrhubott/frigate-helper:v1
    restart: unless-stopped
    environment:
      SERVER: mqtt_broker_address:1883  # Your MQTT broker address
      SUBSCRIBE_TOPIC: frigate/events   # MQTT topic to listen for Frigate events
      PUBLISH_TOPIC: frigate_objects    # MQTT topic to publish processed stats
      CLIENTID: frigate_hass_object_events
      USERNAME: mqtt                    # Add your MQTT username
      PASSWORD: mqtt                    # Add your MQTT password
```

### 3. Run Docker Compose

Then, run the service:

```bash
docker-compose up
```

This will use Docker to spin up the Frigate-Helper service, which will connect to your MQTT broker, subscribe to the Frigate events, and begin monitoring and reporting statistics.

### 4. Verify the Service is Running

You can check that the service is connected and working:

```bash
docker-compose logs -f
```

Make sure that no errors show up and that it successfully subscribes to the relevant MQTT topics.

### 5. Stop the Service

When finished, stop and remove the service using:

```bash
docker-compose down
```

---

## Running Without Docker Compose

If you prefer not to use Docker Compose, you can run the service with only Docker itself by manually specifying the environment variables and running the container. Here's how to set it up:

### Steps

1. **Run Docker Container Manually**:

   You can run the Docker container for Frigate-Helper by using the following command:

   ```bash
   docker run -d \
   --name frigate-helper \
   -e SERVER=mqtt_broker_address:1883 \
   -e SUBSCRIBE_TOPIC=frigate/events \
   -e PUBLISH_TOPIC=frigate_objects \
   -e CLIENTID=frigate_hass_object_events \
   -e USERNAME=mqtt \
   -e PASSWORD=mqtt \
   ghcr.io/jrhubott/frigate-helper:v1
   ```

   Make sure to update the environment variables (`SERVER`, `USERNAME`, `PASSWORD`, etc.) with your actual MQTT broker configuration details.

2. **Check Logs**:

   You can monitor the logs using this command:

   ```bash
   docker logs -f frigate-helper
   ```

3. **Stop and Remove the Container**:

   If needed, you can stop and remove the container:

    ```bash
    docker stop frigate-helper
    docker rm frigate-helper
    ```

### Summary

This method is recommended when you might prefer manual control or can script/manage environment variables directly without needing a `docker-compose` setup.

---

## Running Manually (Without Docker)

If you prefer to run the project without Docker, or if you're in a development environment, you can manually set up and run the Frigate-Helper service using the .NET SDK.

### Steps for Manual Setup

1. **Clone the Project**:

   First, clone the repository from GitHub:

   ```bash
   git clone https://github.com/jrhubott/frigate-helper.git
   cd Frigate-Helper
   ```

2. **Install .NET SDK**:

   Make sure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed, as it's required for running this project.

3. **Set Environment Variables**:

   Before running the app, ensure the correct environment variables are set up. These can be set in a terminal session or specified in an `.env` file or the IDE you are using for development.

   Example for setting environment variables on Linux/macOS:

   ```bash
   export SERVER=mqtt_broker_address:1883
   export SUBSCRIBE_TOPIC=frigate/events
   export PUBLISH_TOPIC=frigate_objects
   export CLIENTID=frigate_hass_object_events
   export USERNAME=mqtt
   export PASSWORD=mqtt
   ```

   Or, on Windows:

   ```bash
   set SERVER=mqtt_broker_address:1883
   set SUBSCRIBE_TOPIC=frigate/events
   set PUBLISH_TOPIC=frigate_objects
   set CLIENTID=frigate_hass_object_events
   set USERNAME=mqtt
   set PASSWORD=mqtt
   ```

4. **Build and Run the Service**:

   You can easily build and run the project using the .NET CLI:

   ```bash
   dotnet build
   dotnet run
   ```

   This will start the Frigate-Helper service and connect it to the configured MQTT broker.

5. **Verify Service in Logs**:

   Check the logs in the terminal where the `.NET` process is running. You should see messages indicating the connection to the broker and subscription to MQTT topics.

### Stopping the Service

To stop the service, press `CTRL+C` in the terminal to exit.

---

## Environment Variables

You can set the following variables in `docker-compose.yml`, the Docker run command, or the local environment for manual deployment:

| Variable              | Description                                              | Default Value             |
| --------------------- | -------------------------------------------------------- | ------------------------- |
| `SERVER`              | Hostname or IP of the MQTT broker                         | `home-assistant.home:1883` |
| `SUBSCRIBE_TOPIC`     | MQTT topic to subscribe for Frigate events                | `frigate/events`           |
| `PUBLISH_TOPIC`       | MQTT topic to publish object statistics or event reports  | `frigate_objects`          |
| `CLIENTID`            | MQTT Client ID                                            | `frigate_hass_object_events`|
| `USERNAME`            | MQTT broker username                                      | `mqtt`                     |
| `PASSWORD`            | MQTT broker password                                      | `mqtt`                     |

---

## MQTT Events Generated

### 1. **Subscribed Events (Input Events)**

- **`frigate/events`**: Listens to events generated by Frigate. The events contain object detection data like the object’s label (e.g., person, car) and information about the zones or camera involved.

### 2. **Published Events (Output Events)**
Frigate-Helper processes subscribed events and publishes the results to MQTT topics. These topics dynamically include the camera, labels, and zones involved.

#### Example Topics:
- `frigate_helper/cameras/living_room/moving`
- `frigate_helper/cameras/living_room/stationary`
- `frigate_helper/cameras/living_room/zones/front_entrance/moving`
- `frigate_helper/cameras/living_room/labels/person/stationary`

#### Example Payload (Moving Objects):

```json
{
  "topic": "frigate_helper/cameras/backyard/moving",
  "payload": "3"
}
```

This message indicates that there are currently 3 moving objects detected by the `backyard` camera.

- **General Object Statistics**:
    - `<base_topic>/moving`: Reports the count of moving objects.
    - `<base_topic>/stationary`: Reports the count of stationary objects.

- **Camera-Specific Events**:
    - `frigate_helper/cameras/<camera_name>/moving`
    - `frigate_helper/cameras/<camera_name>/stationary`

- **Zone-Specific Data**:
    - `frigate_helper/cameras/<camera_name>/zones/<zone_name>/moving`
    - `frigate_helper/cameras/<camera_name>/zones/<zone_name>/stationary`

- **Object Label Events**:
    - `frigate_helper/labels/<label_name>/moving`
    - `frigate_helper/labels/<label_name>/stationary`

---

## Logging

Logging is managed via the .NET logging system and can be configured with log levels in the `appsettings.json` for additional verbosity in debugging.

---

## License

This project is licensed under the MIT License.

---

## Contributing

Contributions are welcome! Feel free to open issues, suggest features, or submit pull requests to improve or extend the functionality of this project.