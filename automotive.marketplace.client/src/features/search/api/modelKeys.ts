export const modelKeys = {
  all: () => ["model"],
  byMakeId: (makeId: string) => [...modelKeys.all(), makeId],
};
