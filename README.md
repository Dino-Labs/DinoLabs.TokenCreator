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

## Generating images based on provided specification

If you want to build images for tokens with specification created in previous command run that command:

```
	DinoLabs.TokenCreator.exe generate -s tokens.json -o Images
```

## Uploading images to IPFS

```
	DinoLabs.TokenCreator.exe upload -s Images -p pinata --apiKey abcd
```