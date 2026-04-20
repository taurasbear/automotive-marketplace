import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { makeFormSchema } from "../schemas/makeFormSchema";
import { MakeFormData } from "../types/MakeFormData";

type MakeFormProps = {
  make: MakeFormData;
  onSubmit: (formData: MakeFormData) => Promise<void>;
  className?: string;
};

const MakeForm = ({ make, onSubmit, className }: MakeFormProps) => {
  const form = useForm({
    defaultValues: { name: make.name },
    resolver: zodResolver(makeFormSchema),
  });

  const handleSubmit = async (formData: MakeFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
        >
          <FormField
            name="name"
            control={form.control}
            render={({ field }) => (
              <FormItem>
                <FormLabel>Make name</FormLabel>
                <FormControl>
                  <Input type="text" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit">Confirm</Button>
        </form>
      </Form>
    </div>
  );
};

export default MakeForm;
