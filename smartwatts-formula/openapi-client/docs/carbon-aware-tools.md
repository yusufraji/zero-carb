# Carbon Aware Tools

## Test Data Generators

There are 2 data generators that help to generate the data files for testing purposes. These created data for all regions in the respective cloud providers, and can be used for demos or recreating more test data.

### AWS Datacenter Test Emissions Generator

The `src\CarbonAware.Tools\CarbonAware.Tools.AWSRegionTestDataGenerator` project generates a json file to be used by the basicJsonPlugin that includes all AWS datacenter regions.

The `aws-regions.json` file it uses is downloaded from the official Amazon Web Services website.

### Azure Datacenter Test Emissions Generator

The `src\CarbonAware.Tools\CarbonAware.Tools.AzureRegionTestDataGenerator` project generates a json file to be used by the basicJsonPlugin that includes all Azure data regions.

The `azure-regions.json` file it uses is generated by the official Microsoft Azure CLI.
