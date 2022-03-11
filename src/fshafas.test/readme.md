# Test

## Test Fixtures

Every method of the HafasClient type has a test fixture of json files:

* the option parameter of the method
* the raw output of the hafas api
* the output of the JavaScript hafas-client api

Every method of the HafasClient type has a test case

* which parses the raw output of the hafas api
* and compares the result with the output of the JavaScript hafas-client.

## Run Tests

* run create-test-fixtures.sh
* run dotnet test
