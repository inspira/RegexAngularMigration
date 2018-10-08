module.exports = {
    "env": {
        "browser": true,
        "es6": false
    },
    "plugins": [
		"angular",
	],
    "parserOptions": {
        "ecmaVersion": 2015
    },
    "rules": {
		"angular/no-http-callback": 2
		"angular/no-services": [2,["uploadFactory","buscaContraparteFactory"]]
    }
};