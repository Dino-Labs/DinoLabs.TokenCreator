# DinoLabs Token Creator

This tools is an utility that helps NFT developers to create tokens. 

As for now it has the following functionality:
* Drawing combindations of token featues
* [In Development] Generating images based on provided specification
* [Planned] Uploading images to IPFS

## Drawing combinations of token features

In order to draw combination for given specification you have to run the following command:

```
DinoLabs.TokenCreator.exe draw -s features.json -o tokens.json
```

Input file has format as below:

```json
{
  "name": "CryptoDino",
  "count": 7000,
  "features": {
    "body": {
      "green": 3,
      "brown": 5,
      "blue": 4,
      "pink": 4,
      "grey": 5
    },
    ...
  }
}
```

As a result you will get *tokens.json* file containing the specificaiton of all tokens combinations.

## Generating images based on provided specification

If you want to build images for tokens with specification created in previous command run that command:

```
DinoLabs.TokenCreator.exe generate -s tokens.json -o Images
```

## Uploading images to IPFS

```
DinoLabs.TokenCreator.exe upload -s Images -p pinata --apiKey abcd
```

## Building and running
To build this tool you need .NET 6 SDK. In order to run the tool you need .NET 6 Runtime