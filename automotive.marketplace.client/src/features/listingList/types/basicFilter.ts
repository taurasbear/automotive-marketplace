import { CarConditionKey } from "@/constants/carConditions";

export type BasicFilter = {
  makeId: string;
  isUsed: CarConditionKey;
  municipalityId: string;
};
