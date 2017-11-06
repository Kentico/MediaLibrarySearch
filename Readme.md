# Search for Media Libraries

This project enables search functionality for Media Libraries in Kentico EMS. It allows searching through binary files.
See full article at https://devnet.kentico.com/articles/search-for-media-libraries

## Getting Started

Start with reading following article on DevNet: https://devnet.kentico.com/articles/search-for-media-libraries

Following instructions will get you a copy of the code files needed to get the search up and running on your local machine for development and testing purposes.

### Prerequisites

Kentico CMS installed on your environment

### Installing

Following steps are describing how to implement search functionality in your instance

```
1) Create a new Class library project
2) Initialize the project to run custom code (see https://docs.kentico.com/k10/custom-development/creating-custom-modules/initializing-modules-to-run-custom-code)
3) Add all code files to that project
4) Reference the project from Kentico instance
```

And then

```
1) Register search index SearchIndex
2) Register scheduled task SearchTaskProcessor
3) Create UI and use SearchResultsTransformation.txt
```
