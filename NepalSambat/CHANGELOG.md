# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-XX

### Added
- Initial release of NepalSambat library
- High-precision astronomical-based Nepal Sambat calendar converter
- Bidirectional conversion between Gregorian and Nepal Sambat dates
- Unlimited year range support (879 CE to infinity)
- Automatic leap year detection and handling
- Comprehensive XML documentation
- Thread-safe implementation
- Zero external dependencies

### Features
- `NepalSambatDate` struct with full comparison operators
- `NepalSambatConverter.FromGregorian()` method
- `NepalSambatConverter.ToGregorian()` method
- `NepalSambatConverter.IsLeapYear()` method
- Meeus' lunar phase algorithm for precise calculations
- Support for all 12 standard months plus leap month "Anala"
- Perfect round-trip conversion accuracy (0.0 days difference)

### Technical Details
- Built for .NET 8.0+
- Comprehensive error handling with meaningful exceptions
- High-performance astronomical calculations
- Clean, maintainable codebase
- Full IntelliSense support with XML documentation 