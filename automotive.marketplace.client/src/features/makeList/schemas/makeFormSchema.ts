import { validation } from "@/utils/validation";
import z from "zod";

export const makeFormSchema = z.object({
  name: z
    .string()
    .min(1, { error: () => validation.required({ label: "Name" }) }),
});
