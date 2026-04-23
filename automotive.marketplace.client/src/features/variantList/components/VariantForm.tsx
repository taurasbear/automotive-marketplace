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
import { useTranslation } from "react-i18next";
import { variantFormSchema } from "../schemas/variantFormSchema";
import { VariantFormData } from "../types/VariantFormData";

type VariantFormProps = {
  variant: VariantFormData;
  onSubmit: (formData: VariantFormData) => Promise<void>;
  className?: string;
};

const VariantForm = ({ variant, onSubmit, className }: VariantFormProps) => {
  const { t } = useTranslation(["admin", "common"]);
  const form = useForm<VariantFormData>({
    defaultValues: variant,
    resolver: zodResolver(variantFormSchema),
  });

  const handleSubmit = async (formData: VariantFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  const selectedMake = form.watch("makeId");
  const prevMakeId = useRef(variant.makeId);

  useEffect(() => {
    if (selectedMake === prevMakeId.current) return;
    prevMakeId.current = selectedMake;
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
                <FormLabel>{t("admin:variants.make")}</FormLabel>
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
                <FormLabel>{t("admin:variants.model")}</FormLabel>
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
            name="fuelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>{t("admin:variants.fuelType")}</FormLabel>
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
                <FormLabel>{t("admin:variants.transmission")}</FormLabel>
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
                <FormLabel>{t("admin:variants.bodyType")}</FormLabel>
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
                <FormLabel>{t("admin:variants.doors")}</FormLabel>
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
                <FormLabel>{t("admin:variants.powerKw")}</FormLabel>
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
                <FormLabel>{t("admin:variants.engineSizeMl")}</FormLabel>
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
                <FormLabel className="mt-0">{t("admin:variants.customVariant")}</FormLabel>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="col-span-4">
            {t("common:actions.confirm")}
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default VariantForm;
