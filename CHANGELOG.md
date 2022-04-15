## 0.1.20 (01-03-2022):

Use new option from the ClusterClient.Core to decrease logs count

## 0.1.20 (01-03-2022):

Bump ClusterConfig dependency version

## 0.1.19 (14-02-2022):

Bump ClusterConfig dependency version

## 0.1.18 (18-01-2022):

Fix bug when ForcedEnvironment was captured once instead of getting per each request.

## 0.1.17 (17-01-2022):

Fix bug when in some case idempotency settings update routine stay wait indefinitely ( PR #[17](https://github.com/vostok/singular.core/pull/17).

## 0.1.16 (06-12-2021):

Added `net6.0` target.

## 0.1.14 (10-11-2021)

Take idempotency settings from the singular instead ClusterConfig.

## 0.1.13 (24-06-2021)

Added replica tags filters filling to singular headers.

## 0.1.12 (27-05-2021)

Add RelativeWeightModifier instead AdaptiveHealthModifier.

## 0.1.11 (30-03-2021)

Customize "local" singular host name and port through environment variables (PR #[11](https://github.com/vostok/clusterclient.singular/pull/11)).

## 0.1.10 (16-03-2021)

Implement service mesh support based on "singular instance per bare metal host" approach (PR #[10](https://github.com/vostok/clusterclient.singular/pull/10)).

## 0.1.9 (17-09-2020)

Using async caches from `vostok.singular.core`.

## 0.1.8 (08-09-2020)

New overload for `SingularClientSettings` constructor - now we can take value for `environmentName` from:
  * `forced.sd.environment` distributed property
  * ClusterConfig client zone

## 0.1.7 (03-09-2020)

IdempotencySettings per environment configuration.

## 0.1.6 (20-07-2020):

Add metrics about the quality of singular work.

## 0.1.5 (22-06-2020):

Cut query params in idempotency check.

## 0.1.4 (02-06-2020):

Check idempotency settings.

## 0.1.2 (27-02-2020):

Prefer local datacenters.

## 0.1.1 (27-01-2020): 

Now filling TargetServiceName and TargetEnvironment properly.

## 0.1.0 (21-05-2019): 

Initial prerelease.
