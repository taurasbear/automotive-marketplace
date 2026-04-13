import z from "zod";

export const modelFormSchema = z.object({
  name: z.string(),
  makeId: z.string(),
});
