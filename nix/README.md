# Nix packaging notes

`deps.json` lists the NuGet packages the build downloads ahead of time, since the Nix sandbox has no
network. Regenerate it whenever a project reference changes or the nixpkgs pin moves:

```sh
nix build '.#keysharp.fetch-deps'
./result nix/deps.json
nix build .#keysharp
```

Use that command rather than editing the file by hand. It knows which packages the .NET SDK already
provides, and listing one of those here fails the build.

`flake.nix` pins nixpkgs and Eto to exact revisions. Every other packager builds the tip of Eto's
`Keysharp` branch, so move the Eto pin when that moves.

CI has a NixOS leg that builds the package and checks what it contains, but it is not part of the push
gate: run the CI workflow manually and pick `nixos` (or `all`) after touching these expressions, a project
reference, or the install payload. The same thing locally is:

```sh
nix flake check -L --all-systems
nix build .#keysharp
```

`docs/linux-nixos.md` covers installing and running Keysharp on NixOS.
