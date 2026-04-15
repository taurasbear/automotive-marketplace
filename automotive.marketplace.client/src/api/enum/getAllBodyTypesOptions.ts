// TODO: Used by the Create/Edit Variant form (not yet implemented)
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { BodyType } from "@/types/enum/BodyType";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getAllBodyTypes = () =>
  axiosClient.get<BodyType[]>(ENDPOINTS.BODY_TYPE.GET_ALL);

export const getAllBodyTypesOptions = queryOptions({
  queryKey: enumKeys.bodyTypeEntities(),
  queryFn: getAllBodyTypes,
});
