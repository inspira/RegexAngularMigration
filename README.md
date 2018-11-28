# Refactor $http's deprecated code to new standard (AngularJS)

Due to [b54a39](https://github.com/angular/angular.js/commit/b54a39e2029005e0572fbd2ac0e8f6a4e5d69014), $http's deprecated custom callback methods - .success() and .error() - have been removed. You can use the standard .then()/.catch() promise methods instead, but note that the method signatures and return values are different.

This console project uses Regex to refactor the deprecated promise methods to the new standard.

## Quick Guide

- Use App.config to set the folder where the AngularJS code with deprecated methods are located.
- Install BeyondCompare to help you compare the deprecated code with the refactored one. This is optional but highly recommended.
- To help you check if some refactored code broke your AngularJS / Javascript code, you can install and run JSHint and ESLint using the configuration files located in \SupportFiles folder.
    - The ESLint will check if all deprecated code were refactored.
    - The JSHint will check if the refactored code broke the JS code.

