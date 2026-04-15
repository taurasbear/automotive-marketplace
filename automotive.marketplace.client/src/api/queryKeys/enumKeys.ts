export const enumKeys = {
  all: () => ["enum"],
  transmissionTypes: () => [...enumKeys.all(), "tranmissionTypes"],
  fuelTypes: () => [...enumKeys.all(), "fuelTypes"],
  bodyTypes: () => [...enumKeys.all(), "bodyTypes"],
  drivetrainTypes: () => [...enumKeys.all(), "drivetrainTypes"],
  fuels: () => [...enumKeys.all(), "fuels"],
  transmissions: () => [...enumKeys.all(), "transmissions"],
  bodyTypeEntities: () => [...enumKeys.all(), "bodyTypeEntities"],
  drivetrains: () => [...enumKeys.all(), "drivetrains"],
};
