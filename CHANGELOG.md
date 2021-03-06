# Changelog for janovrom.firesimulation
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Performance

## [1.0.0]
### Added
- Code documentation

## [0.9.4-pre]
### Fixed
- Editor assemblies should now be included only in editor

## [0.9.3-pre]
### Moved
- Scene with UI moved to samples to allow editing after import.

### Added
- Added explanation to readme.

### Removed
- ChangeText component that was left out when moving to BoolBinding.

## [0.9.2-pre]
## Removed
- Asset files from samples. Samples now only contain scene with setup for given level.

## [0.9.1-pre]
### Added
- Samples.
- Binding for running simulation.

## [0.9.0-pre]
### Added
- Simple event system based on ScriptableObject.
- Feature-wise complete fire simulation.
- Material GPU Instancing used for rendering plants.