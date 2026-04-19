import BodyTypeSelect from "@/components/forms/select/BodyTypeSelect";
import FuelSelect from "@/components/forms/select/FuelSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import TransmissionToggleGroup from "@/components/forms/TransmissionToggleGroup";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
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
import { useEffect, useRef } from "react";
import { useForm } from "react-hook-form";
import { variantFormSchema } from "../schemas/variantFormSchema";
import { VariantFormData } from "../types/VariantFormData";

type VariantFormProps = {
  variant: VariantFormData;
  onSubmit: (formData: VariantFormData) => Promise<void>;
  className?: string;
};

const VariantForm = ({ variant, onSubmit, className }: VariantFormProps) => {
  const form = useForm<VariantFormData>({
    defaultValues: variant,
    resolver: zodResolver(variantFormSchema),
  });

  const handleSubmit = async (formData: VariantFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  const selectedMake = form.watch("makeId");
  const isMounted = useRef(false);

  useEffect(() => {
    if (!isMounted.current) {
      isMounted.current = true;
      return;
    }
    form.setValue("modelId", "");
  }, [selectedMake]);

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs grid-cols-4 gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
        >
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={field.onChange}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="modelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={field.onChange}
                    selectedMake={selectedMake}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={1900} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="fuelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Fuel type*</FormLabel>
                <FormControl>
                  <FuelSelect onValueChange={field.onChange} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="transmissionId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Transmission*</FormLabel>
                <FormControl>
                  <TransmissionToggleGroup
                    type="single"
                    onValueChange={field.onChange}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="bodyTypeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Body type*</FormLabel>
                <FormControl>
                  <BodyTypeSelect onValueChange={field.onChange} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="doorCount"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start">
                <FormLabel>Doors*</FormLabel>
                <FormControl>
                  <Input type="number" min={1} max={9} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="powerKw"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start">
                <FormLabel>Power (kW)*</FormLabel>
                <FormControl>
                  <Input type="number" min={5} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="engineSizeMl"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Engine size (ml)*</FormLabel>
                <FormControl>
                  <Input type="number" min={300} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isCustom"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-row items-center gap-2">
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
                <FormLabel className="mt-0">Custom variant</FormLabel>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="col-span-4">
            Confirm
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default VariantForm;
