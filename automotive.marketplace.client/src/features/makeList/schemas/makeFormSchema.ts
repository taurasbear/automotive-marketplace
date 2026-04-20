import z from "zod";

export const makeFormSchema = z.object({
  name: z.string().min(1, "Name is required"),
});
