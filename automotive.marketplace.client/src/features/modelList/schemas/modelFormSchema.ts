import z from "zod";

export const modelFormSchema = z.object({
  name: z.string(),
  firstAppearanceDate: z.date(),
  isDiscontinued: z.boolean(),
  makeId: z.string(),
});
