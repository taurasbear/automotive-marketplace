export const modelKeys = {
  all: () => ["model"],
  byMakeId: (makeId: string, onlyWithListings?: boolean) =>
    [...modelKeys.all(), makeId, { onlyWithListings }],
  byId: (id: string) => [...modelKeys.all(), id],
};
