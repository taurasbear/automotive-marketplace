export const CAR_CONDITION_OPTIONS = [
  ["new", "New"],
  ["used", "Used"],
  ["newused", "Used & New"],
] as const;

export type CarConditionKey = (typeof CAR_CONDITION_OPTIONS)[number][0];
