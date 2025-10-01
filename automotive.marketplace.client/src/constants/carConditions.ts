export const CAR_CONDITION_OPTIONS = [
  ["new", "New"],
  ["used", "Used"],
  ["newUsed", "Used & New"],
] as const;

export type CarConditionKey = (typeof CAR_CONDITION_OPTIONS)[number][0];
