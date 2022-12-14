# openapi-client
No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)

This Python package is automatically generated by the [OpenAPI Generator](https://openapi-generator.tech) project:

- API version: 1.0
- Package version: 1.0.0
- Build package: org.openapitools.codegen.languages.PythonClientCodegen

## Requirements.

Python >=3.6

## Installation & Usage
### pip install

If the python package is hosted on a repository, you can install directly using:

```sh
pip install git+https://github.com/GIT_USER_ID/GIT_REPO_ID.git
```
(you may need to run `pip` with root permission: `sudo pip install git+https://github.com/GIT_USER_ID/GIT_REPO_ID.git`)

Then import the package:
```python
import openapi_client
```

### Setuptools

Install via [Setuptools](http://pypi.python.org/pypi/setuptools).

```sh
python setup.py install --user
```
(or `sudo python setup.py install` to install the package for all users)

Then import the package:
```python
import openapi_client
```

## Getting Started

Please follow the [installation procedure](#installation--usage) and then run the following:

```python

import time
import openapi_client
from pprint import pprint
from openapi_client.api import carbon_aware_api
from openapi_client.model.carbon_intensity_batch_dto import CarbonIntensityBatchDTO
from openapi_client.model.carbon_intensity_dto import CarbonIntensityDTO
from openapi_client.model.emissions_data import EmissionsData
from openapi_client.model.emissions_forecast_batch_dto import EmissionsForecastBatchDTO
from openapi_client.model.emissions_forecast_dto import EmissionsForecastDTO
from openapi_client.model.validation_problem_details import ValidationProblemDetails
# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)



# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = carbon_aware_api.CarbonAwareApi(api_client)
    emissions_forecast_batch_dto = [
        EmissionsForecastBatchDTO(
            requested_at=dateutil_parser('2022-06-01T00:03:30Z'),
            data_start_at=dateutil_parser('2022-06-01T12:00:00Z'),
            data_end_at=dateutil_parser('2022-06-01T18:00:00Z'),
            window_size=30,
            location="eastus",
        ),
    ] # [EmissionsForecastBatchDTO] | Array of requested forecasts. (optional)

    try:
        # Given an array of historical forecasts, retrieves the data that contains  forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided.
        api_response = api_instance.batch_forecast_data_async(emissions_forecast_batch_dto=emissions_forecast_batch_dto)
        pprint(api_response)
    except openapi_client.ApiException as e:
        print("Exception when calling CarbonAwareApi->batch_forecast_data_async: %s\n" % e)
```

## Documentation for API Endpoints

All URIs are relative to *http://localhost*

Class | Method | HTTP request | Description
------------ | ------------- | ------------- | -------------
*CarbonAwareApi* | [**batch_forecast_data_async**](docs/CarbonAwareApi.md#batch_forecast_data_async) | **POST** /emissions/forecasts/batch | Given an array of historical forecasts, retrieves the data that contains  forecasts metadata, the optimal forecast and a range of forecasts filtered by the attributes [start...end] if provided.
*CarbonAwareApi* | [**get_average_carbon_intensity**](docs/CarbonAwareApi.md#get_average_carbon_intensity) | **GET** /emissions/average-carbon-intensity | Retrieves the measured carbon intensity data between the time boundaries and calculates the average carbon intensity during that period.
*CarbonAwareApi* | [**get_average_carbon_intensity_batch**](docs/CarbonAwareApi.md#get_average_carbon_intensity_batch) | **POST** /emissions/average-carbon-intensity/batch | Given an array of request objects, each with their own location and time boundaries, calculate the average carbon intensity for that location and time period   and return an array of carbon intensity objects.
*CarbonAwareApi* | [**get_best_emissions_data_for_locations_by_time**](docs/CarbonAwareApi.md#get_best_emissions_data_for_locations_by_time) | **GET** /emissions/bylocations/best | Calculate the best emission data by list of locations for a specified time period.
*CarbonAwareApi* | [**get_current_forecast_data**](docs/CarbonAwareApi.md#get_current_forecast_data) | **GET** /emissions/forecasts/current | Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window.
*CarbonAwareApi* | [**get_emissions_data_for_location_by_time**](docs/CarbonAwareApi.md#get_emissions_data_for_location_by_time) | **GET** /emissions/bylocation | Calculate the best emission data by location for a specified time period.
*CarbonAwareApi* | [**get_emissions_data_for_locations_by_time**](docs/CarbonAwareApi.md#get_emissions_data_for_locations_by_time) | **GET** /emissions/bylocations | Calculate the observed emission data by list of locations for a specified time period.


## Documentation For Models

 - [CarbonIntensityBatchDTO](docs/CarbonIntensityBatchDTO.md)
 - [CarbonIntensityDTO](docs/CarbonIntensityDTO.md)
 - [EmissionsData](docs/EmissionsData.md)
 - [EmissionsDataDTO](docs/EmissionsDataDTO.md)
 - [EmissionsForecastBatchDTO](docs/EmissionsForecastBatchDTO.md)
 - [EmissionsForecastDTO](docs/EmissionsForecastDTO.md)
 - [ValidationProblemDetails](docs/ValidationProblemDetails.md)


## Documentation For Authorization

 All endpoints do not require authorization.

## Author




## Notes for Large OpenAPI documents
If the OpenAPI document is large, imports in openapi_client.apis and openapi_client.models may fail with a
RecursionError indicating the maximum recursion limit has been exceeded. In that case, there are a couple of solutions:

Solution 1:
Use specific imports for apis and models like:
- `from openapi_client.api.default_api import DefaultApi`
- `from openapi_client.model.pet import Pet`

Solution 2:
Before importing the package, adjust the maximum recursion limit as shown below:
```
import sys
sys.setrecursionlimit(1500)
import openapi_client
from openapi_client.apis import *
from openapi_client.models import *
```

