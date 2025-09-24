export const modelKeys = {
  all: () => ["model"],
  byMakeId: (makeId: string) => [...modelKeys.all(), makeId],
  byId: (id: string) => [...modelKeys.all(), id],
};
