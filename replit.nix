{pkgs}: {
  deps = [
    pkgs.haskellPackages.mpi-hs
    pkgs.unzipNLS
    pkgs.wget
    pkgs.dotnetCorePackages.sdk_7_0_3xx
  ];
}
