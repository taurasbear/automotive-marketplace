export const getPriceRange = (): string[] => {
  const prices = ["150", "300", "500"];
  for (let i = 1000; i < 5000; i += 500) {
    prices.push(i.toString());
  }
  for (let i = 5000; i < 10000; i += 1000) {
    prices.push(i.toString());
  }
  for (let i = 10000; i <= 100000; i += 2500) {
    prices.push(i.toString());
  }

  return prices;
};

export const getMileageRange = (): string[] => {
  const mileages = ["0", "2500", "5000"];
  for (let i = 10000; i < 100_000; i += 10_000) {
    mileages.push(i.toString());
  }
  for (let i = 100_000; i < 1_000_000; i += 20_000) {
    mileages.push(i.toString());
  }

  return mileages;
};

export const getPowerRange = (): string[] => {
  const powerValues = ["1"];
  for (let i = 10; i < 100; i += 10) {
    powerValues.push(i.toString());
  }
  for (let i = 100; i <= 400; i += 20) {
    powerValues.push(i.toString());
  }

  return powerValues;
};

export const getYearRange = (): string[] => {
  const years = [];
  const currentYear = new Date().getUTCFullYear();
  for (let i = 1950; i <= currentYear; i += 1) {
    years.push(i.toString());
  }

  return years;
};
