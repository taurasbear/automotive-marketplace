export const carKeys = {
  all: () => ["car"],
  byMakeId: (makeId: string) => [...carKeys.all(), makeId],
  byId: (id: string) => [...carKeys.all(), id],
};
