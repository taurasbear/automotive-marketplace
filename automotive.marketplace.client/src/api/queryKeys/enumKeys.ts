export const enumKeys = {
  all: () => ["enum"],
  transmissionTypes: () => [...enumKeys.all(), "tranmissionTypes"],
  fuelTypes: () => [...enumKeys.all(), "fuelTypes"],
  bodyTypes: () => [...enumKeys.all(), "bodyTypes"],
  drivetrainTypes: () => [...enumKeys.all(), "drivetrainTypes"],
};
