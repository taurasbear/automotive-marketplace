import z from "zod";
import { makeFormSchema } from "../schemas/makeFormSchema";

export type MakeFormData = z.infer<typeof makeFormSchema>;
